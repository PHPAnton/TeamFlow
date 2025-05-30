<template>
    <div class="tasks-container">
        <div class="header">
            <h1>Задачи</h1>
            <button @click="openCreateForm">+ Новая задача</button>
        </div>

        <div class="filters">
            <label>
                Приоритет:
                <select v-model="filters.priority">
                    <option value="">Все</option>
                    <option>Low</option>
                    <option>Medium</option>
                    <option>High</option>
                </select>
            </label>

            <label>
                Статус:
                <select v-model="filters.status">
                    <option value="">Все</option>
                    <option>New</option>
                    <option>InProgress</option>
                    <option>Completed</option>
                </select>
            </label>
        </div>

        <div class="task-columns">
            <div class="column" v-for="(list, status) in filteredGrouped" :key="status">
                <h2>{{ statusLabels[status] }}</h2>
                <draggable :list="grouped[status]"
                           :group="{ name: 'tasks', pull: true, put: true }"
                           item-key="id"
                           @change="event => onDrop(event, status)">
                    <template #item="{ element: task }">
                        <div class="task-card">
                            <h3>{{ task.title }}</h3>
                            <p>{{ task.description }}</p>
                            <small>Приоритет: {{ task.priority }}</small><br />
                            <small>Дедлайн: {{ formatDate(task.deadline) }}</small>
                            <div class="tags" v-if="task.tags?.length">
                                <span class="tag" v-for="tag in task.tags" :key="tag">{{ tag }}</span>
                            </div>
                            <div class="actions">
                                <button @click="openEditForm(task)">✏</button>
                                <button @click="deleteTask(task.id)">🗑</button>
                            </div>
                        </div>
                    </template>
                </draggable>
            </div>
        </div>

        <!-- Модальное окно -->
        <div v-if="showForm" class="modal-overlay">
            <div class="modal">
                <h2>{{ editMode ? 'Редактировать' : 'Создать' }} задачу</h2>
                <form @submit.prevent="submitForm">
                    <label>Заголовок:</label>
                    <input v-model="form.title" required />

                    <label>Описание:</label>
                    <textarea v-model="form.description"></textarea>

                    <label>Проект:</label>
                    <select v-model="selectedProjectId" @change="loadTasks">
                        <option v-for="project in projects" :key="project.id" :value="project.id">
                            {{ project.title }}
                        </option>
                    </select>

                    <div class="new-project-input">
                        <input v-model="newProjectName" placeholder="Новый проект..." />
                        <button type="button" @click="addProject">Создать</button>
                    </div>

                    <label>Приоритет:</label>
                    <select v-model="form.priority">
                        <option>Low</option>
                        <option>Medium</option>
                        <option>High</option>
                    </select>

                    <label>Статус:</label>
                    <select v-model="form.status">
                        <option>New</option>
                        <option>InProgress</option>
                        <option>Completed</option>
                    </select>

                    <label>Дедлайн:</label>
                    <input type="date" v-model="form.deadline" />

                    <label>Метки (через запятую):</label>
                    <input v-model="form.tagsText" placeholder="дизайн, urgent" />

                    <div class="form-buttons">
                        <button type="submit">{{ editMode ? 'Сохранить' : 'Создать' }}</button>
                        <button type="button" @click="closeForm">Отмена</button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>

<script setup>
    import { ref, onMounted, computed } from 'vue';
    import draggable from 'vuedraggable';
    import api from '@/axios';

    const tasks = ref([]);
    const projects = ref([]);
    const selectedProjectId = ref('');
    const newProjectName = ref('');
    const grouped = ref({ New: [], InProgress: [], Completed: [] });
    const filters = ref({ priority: '', status: '' });
    const showForm = ref(false);
    const editMode = ref(false);
    const currentId = ref(null);

    const form = ref({
        title: '',
        description: '',
        status: 'New',
        priority: 'Medium',
        deadline: '',
        tagsText: '',
    });

    const statusLabels = {
        New: 'Новые',
        InProgress: 'В процессе',
        Completed: 'Завершено',
    };

    const loadProjects = async () => {
        const res = await api.get('/projects');
        projects.value = res.data;
        if (projects.value.length && !selectedProjectId.value) {
            selectedProjectId.value = projects.value[0].id;
            await loadTasks();
        }
    };

    const loadTasks = async () => {
        const res = await api.get('/tasks');
        tasks.value = res.data.filter(t => t.projectId === selectedProjectId.value);
        groupTasks();
    };

    const groupTasks = () => {
        grouped.value = { New: [], InProgress: [], Completed: [] };
        for (const task of tasks.value) {
            task.tags = task.tags || [];
            grouped.value[task.status].push(task);
        }
    };

    const filteredGrouped = computed(() => {
        const res = {};
        for (const status in grouped.value) {
            res[status] = grouped.value[status].filter(task => {
                const priorityMatch = !filters.value.priority || task.priority === filters.value.priority;
                const statusMatch = !filters.value.status || task.status === filters.value.status;
                return priorityMatch && statusMatch;
            });
        }
        return res;
    });

    const formatDate = (dateStr) => {
        if (!dateStr) return '—';
        const date = new Date(dateStr);
        return date.toLocaleDateString();
    };

    const openCreateForm = () => {
        editMode.value = false;
        currentId.value = null;
        form.value = {
            title: '', description: '', status: 'New', priority: 'Medium', deadline: '', tagsText: ''
        };
        showForm.value = true;
    };

    const openEditForm = (task) => {
        editMode.value = true;
        currentId.value = task.id;
        form.value = {
            title: task.title,
            description: task.description,
            status: task.status,
            priority: task.priority,
            deadline: task.deadline?.slice(0, 10) || '',
            tagsText: task.tags?.join(', ') || ''
        };
        showForm.value = true;
    };

    const closeForm = () => showForm.value = false;

    const submitForm = async () => {
        if (!selectedProjectId.value) return alert('Выберите проект!');

        const payload = {
            title: form.value.title,
            description: form.value.description,
            status: form.value.status,
            priority: form.value.priority,
            deadline: form.value.deadline || null,
            tags: form.value.tagsText.split(',').map(t => t.trim()).filter(Boolean),
            projectId: selectedProjectId.value,
        };

        if (editMode.value && currentId.value) {
            await api.put(`/tasks/${currentId.value}`, payload);
        } else {
            await api.post('/tasks', payload);
        }
        closeForm();
        await loadTasks();
    };

    const deleteTask = async (id) => {
        if (confirm('Удалить задачу?')) {
            await api.delete(`/tasks/${id}`);
            await loadTasks();
        }
    };

    const onDrop = async (event, newStatus) => {
        const movedTask = event.moved?.element;
        if (!movedTask || movedTask.status === newStatus) return;

        movedTask.status = newStatus;
        await api.put(`/tasks/${movedTask.id}`, movedTask);
        await loadTasks();
    };

    const addProject = async () => {
        if (!newProjectName.value.trim()) return;
        try {
            const token = localStorage.getItem('token');
            const tokenPayload = JSON.parse(atob(token.split('.')[1]));
            const userId = tokenPayload.sub || tokenPayload.id;

            const res = await api.post('/projects', {
                title: newProjectName.value,
                description: '',
                ownerId: userId
            });

            projects.value.push(res.data);
            selectedProjectId.value = res.data.id;
            newProjectName.value = '';
            await loadTasks();
        } catch (err) {
            console.error('Ошибка проекта:', err);
            alert('Ошибка при создании проекта.');
        }
    };

    onMounted(loadProjects);
</script>

<style scoped>
    .tasks-container {
        padding: 30px;
        max-width: 1200px;
        margin: 0 auto;
        color: white;
    }

    .header {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .filters {
        display: flex;
        gap: 20px;
        margin: 20px 0;
        align-items: center;
    }

    .task-columns {
        display: flex;
        gap: 20px;
    }

    .column {
        flex: 1;
        background: #2e2e3e;
        padding: 20px;
        border-radius: 8px;
        min-height: 300px;
        transition: background 0.3s;
    }

    .task-card {
        background: #3e3e50;
        padding: 10px;
        margin-top: 10px;
        border-radius: 4px;
        position: relative;
        transition: transform 0.2s;
    }

        .task-card:hover {
            transform: scale(1.02);
        }

    .actions {
        position: absolute;
        top: 8px;
        right: 8px;
        display: flex;
        gap: 6px;
    }

    .tags {
        margin-top: 8px;
    }

    .tag {
        background: #555;
        border-radius: 4px;
        padding: 2px 6px;
        margin-right: 4px;
        font-size: 12px;
    }

    button {
        background: #4a90e2;
        border: none;
        padding: 6px 12px;
        color: white;
        font-weight: bold;
        border-radius: 4px;
        cursor: pointer;
    }

    .modal-overlay {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: #000000aa;
        display: flex;
        justify-content: center;
        align-items: center;
    }

    .modal {
        background: #1e1e2f;
        padding: 20px;
        border-radius: 8px;
        width: 400px;
    }

    label {
        display: block;
        margin-top: 10px;
    }

    input, textarea, select {
        width: 100%;
        margin-top: 5px;
        padding: 6px;
        background: #2e2e3e;
        border: none;
        border-radius: 4px;
        color: white;
    }

    .form-buttons {
        margin-top: 15px;
        display: flex;
        justify-content: space-between;
    }
</style>
