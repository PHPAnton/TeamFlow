<template>
    <div class="tasks-container">
        <div class="header">
            <h1>Задачи</h1>
            <div>
                <button @click="showMembersModal = true" class="btn btn-outline-info me-2">👥 Участники</button>
                <button @click="openCreateForm" class="btn btn-primary">+ Новая задача</button>
            </div>
        </div>

        <ProjectMembersModal v-if="showMembersModal"
                             :projects="projects"
                             @close="showMembersModal = false" />
        <ProjectSelect :projects="projects" v-model="selectedProjectId" />

        <div class="filters">
            <label for="priorityFilter">
                Приоритет:
                <select id="priorityFilter" v-model="filters.priority" class="form-select filter-select">
                    <option value="">Все</option>
                    <option>Low</option>
                    <option>Medium</option>
                    <option>High</option>
                </select>
            </label>

            <label for="statusFilter">
                Статус:
                <select id="statusFilter" v-model="filters.status" class="form-select filter-select">
                    <option value="">Все</option>
                    <option>New</option>
                    <option>InProgress</option>
                    <option>Completed</option>
                </select>
            </label>
        </div>

        <div class="task-columns">
            <div class="column" v-for="status in statuses" :key="status">
                <h2>{{ statusLabels[status] }}</h2>
                <draggable :list="groupedTasks[status]"
                           :group="{ name: 'tasks', pull: true, put: true }"
                           item-key="id"
                           @end="event => onDrop(status, event)">
                    <template #item="{ element: task }">
                        <div class="task-card">
                            <h3>{{ task.title }}</h3>
                            <p>{{ task.description }}</p>
                            <small>Приоритет: {{ task.priority }}</small><br />
                            <small>Дедлайн: {{ formatDate(task.deadline) }}</small>
                            <div class="tags" v-if="task.tags?.length">
                                <span class="tag" v-for="tag in task.tags" :key="tag">{{ tag }}</span>
                            </div>
                            <div v-if="task.assignedUser" class="assigned-user">
                                👤 <b>{{ task.assignedUser.username }}</b>
                                <span class="email">({{ task.assignedUser.email }})</span>
                            </div>
                            <div class="actions">
                                <button @click="openEditForm(task)" class="btn btn-sm btn-outline-light me-1">✏</button>
                                <button @click="deleteTask(task.id)" class="btn btn-sm btn-outline-danger">🗑</button>
                            </div>
                        </div>
                    </template>
                </draggable>
            </div>
        </div>

        <!-- Модальное окно задач -->
        <Teleport to="body">
            <div v-if="showForm" class="custom-modal-overlay" @click.self="closeForm">
                <div class="custom-modal">
                    <div class="modal-header">
                        <h2>{{ editMode ? 'Редактировать' : 'Создать' }} задачу</h2>
                        <button class="close-btn" @click="closeForm">&times;</button>
                    </div>
                    <form @submit.prevent="submitForm" class="modal-form">
                        <div class="mb-3">
                            <label for="title" class="form-label">Заголовок:</label>
                            <input id="title" name="title" v-model="form.title" required class="form-control input-field" placeholder="Введите заголовок" />
                        </div>
                        <div class="mb-3">
                            <label for="desc" class="form-label">Описание:</label>
                            <textarea id="desc" name="description" v-model="form.description" class="form-control input-field" rows="3"></textarea>
                        </div>
                        <div class="mb-3">
                            <label for="project" class="form-label">Проект:</label>
                            <select id="project" name="project" v-model="selectedProjectId" @change="onProjectChange" class="form-select input-field">
                                <option v-for="project in projects" :key="project.id" :value="project.id">{{ project.title }}</option>
                            </select>
                        </div>
                        <div class="new-project-input mb-3">
                            <label for="newProjectName" class="form-label">Новый проект:</label>
                            <div class="input-group input-field">
                                <input id="newProjectName" name="newProjectName" v-model="newProjectName" placeholder="Новый проект..." class="form-control" />
                                <button type="button" @click="addProject" class="btn btn-outline-secondary">Создать</button>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label for="priority" class="form-label">Приоритет:</label>
                                <select id="priority" name="priority" v-model="form.priority" class="form-select input-field">
                                    <option>Low</option>
                                    <option>Medium</option>
                                    <option>High</option>
                                </select>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label for="status" class="form-label">Статус:</label>
                                <select id="status" name="status" v-model="form.status" class="form-select input-field">
                                    <option>New</option>
                                    <option>InProgress</option>
                                    <option>Completed</option>
                                </select>
                            </div>
                        </div>
                        <div class="mb-3">
                            <label for="deadline" class="form-label">Дедлайн:</label>
                            <input id="deadline" name="deadline" type="date" v-model="form.deadline" class="form-control input-field" />
                        </div>
                        <div class="mb-3">
                            <label for="assignee" class="form-label">Назначить на:</label>
                            <select id="assignee" name="assignee" v-model="form.assignedUserId" class="form-select input-field">
                                <option :value="null">— Не назначено —</option>
                                <option v-for="member in membersForProject" :key="member.userId" :value="member.userId">
                                    {{ member.username }} ({{ member.email }})
                                </option>
                            </select>
                        </div>
                        <div class="mb-3">
                            <label for="tags" class="form-label">Метки (через запятую):</label>
                            <input id="tags" name="tags" v-model="form.tagsText" placeholder="дизайн, urgent" class="form-control input-field" />
                        </div>
                        <div class="d-flex justify-content-end gap-2">
                            <button type="button" @click="closeForm" class="btn btn-secondary">Отмена</button>
                            <button type="submit" class="btn btn-success">{{ editMode ? 'Сохранить' : 'Создать' }}</button>
                        </div>
                    </form>
                </div>
            </div>
        </Teleport>
    </div>
</template>

<script setup>
    import { ref, computed, onMounted, watch } from 'vue';
    import draggable from 'vuedraggable';
    import api from '@/axios';
    import ProjectMembersModal from '@/components/ProjectMembersModal.vue';
    import ProjectSelect from '@/components/ProjectSelect.vue';

    const tasks = ref([]);
    const projects = ref([]);
    const selectedProjectId = ref('');
    const newProjectName = ref('');
    const filters = ref({ priority: '', status: '' });
    const showForm = ref(false);
    const showMembersModal = ref(false);
    const editMode = ref(false);
    const currentId = ref(null);

    const form = ref({
        title: '',
        description: '',
        status: 'New',
        priority: 'Medium',
        deadline: '',
        tagsText: '',
        assignedUserId: null,
    });

    const statusLabels = {
        New: 'Новые',
        InProgress: 'В процессе',
        Completed: 'Завершено',
    };
    const statuses = ['New', 'InProgress', 'Completed'];

    // === MEMBERS ===
    const members = ref([]);
    const membersForProject = computed(() => members.value);

    const loadMembers = async (projectId) => {
        members.value = [];
        if (!projectId) return;
        try {
            const token = localStorage.getItem('token');
            const response = await api.get(`/ProjectMembers/${projectId}`, {
                headers: { Authorization: `Bearer ${token}` }
            });
            members.value = response.data.map(x => ({
                userId: x.userId,
                username: x.username,
                email: x.email,
            }));
        } catch {
            members.value = [];
        }
    };

    // === TASKS ===
    const loadProjects = async () => {
        const res = await api.get('/projects');
        projects.value = res.data;
        if (projects.value.length && !selectedProjectId.value) {
            selectedProjectId.value = projects.value[0].id;
        }
    };

    const loadTasks = async () => {
        const res = await api.get('/tasks');
        tasks.value = res.data;
    };

    onMounted(async () => {
        await loadProjects();
        if (selectedProjectId.value) {
            await loadMembers(selectedProjectId.value);
            await loadTasks();
        }
    });

    watch(selectedProjectId, async (val) => {
        if (val) {
            await loadMembers(val);
            await loadTasks();
        }
    });

    // === Группировка и фильтрация для отображения и drag&drop ===
    const groupedTasks = computed(() => {
        // Только для выбранного проекта
        const projectTasks = tasks.value.filter(
            t => t.project && t.project.id === selectedProjectId.value
        );
        // Фильтруем по приоритету и статусу
        const filtered = projectTasks.filter(task => {
            const priorityMatch = !filters.value.priority || task.priority === filters.value.priority;
            const statusMatch = !filters.value.status || task.status === filters.value.status;
            return priorityMatch && statusMatch;
        });
        // Группируем по статусу
        const res = { New: [], InProgress: [], Completed: [] };
        for (const task of filtered) {
            if (res[task.status]) res[task.status].push(task);
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
            title: '',
            description: '',
            status: 'New',
            priority: 'Medium',
            deadline: '',
            tagsText: '',
            assignedUserId: null,
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
            tagsText: task.tags?.join(', ') || '',
            assignedUserId: task.assignedUserId ?? null,
        };
        showForm.value = true;
    };

    const closeForm = () => (showForm.value = false);

    const submitForm = async () => {
        if (!selectedProjectId.value) return alert('Выберите проект!');
        const payload = {
            title: form.value.title,
            description: form.value.description,
            status: form.value.status,
            priority: form.value.priority,
            deadline: form.value.deadline || null,
            tags: form.value.tagsText.split(',').map((t) => t.trim()).filter(Boolean),
            projectId: selectedProjectId.value,
            assignedUserId: form.value.assignedUserId || null,
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

    // === Drag and Drop — при завершении перетаскивания меняем статус через API ===
    const onDrop = async (newStatus, event) => {
        // В библиотеке vuedraggable именно событие end гарантирует, что все таски в массиве уже переставлены
        // Нам нужно найти таск, который теперь оказался в колонке с новым статусом, и обновить его
        // Перемещаем только если была реально смена статуса
        if (!event || !event.item || !event.item.id) return;
        const movedTaskId = event.item.id;
        const movedTask = tasks.value.find(t => t.id === movedTaskId);
        if (!movedTask || movedTask.status === newStatus) return;
        try {
            await api.put(`/tasks/${movedTask.id}`, {
                ...movedTask,
                status: newStatus,
                assignedUserId: movedTask.assignedUserId ?? null,
                projectId: movedTask.project.id,
            });
            await loadTasks();
        } catch {
            alert('Не удалось обновить статус задачи.');
        }
    };

    // Создать проект
    const addProject = async () => {
        if (!newProjectName.value.trim()) return;
        try {
            const token = localStorage.getItem('token');
            const tokenPayload = JSON.parse(atob(token.split('.')[1]));
            const userId = tokenPayload.sub || tokenPayload.id;
            const res = await api.post('/projects', {
                title: newProjectName.value,
                description: '',
                ownerId: userId,
            });
            projects.value.push(res.data);
            selectedProjectId.value = res.data.id;
            newProjectName.value = '';
            await loadTasks();
        } catch {
            alert('Ошибка при создании проекта.');
        }
    };

    const onProjectChange = async () => {
        await loadMembers(selectedProjectId.value);
        await loadTasks();
    };
</script>


<style scoped>
    .tasks-container {
        padding: 30px;
        max-width: 1200px;
        margin: 0 auto;
        color: white;
        background: #12121e;
        min-height: 100vh;
    }

    .header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 20px;
    }

    .filters {
        display: flex;
        gap: 20px;
        margin: 20px 0;
        align-items: center;
    }

    .filter-select {
        background-color: #1e1e2f;
        color: #f1f1f1;
        border: 1px solid #444;
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

        .column h2 {
            margin-bottom: 15px;
        }

    .task-card {
        background: #3e3e50;
        padding: 15px;
        margin-top: 10px;
        border-radius: 6px;
        position: relative;
        transition: transform 0.2s, background 0.3s;
    }

        .task-card:hover {
            transform: translateY(-2px);
            background: #4e4e62;
        }

    .actions {
        position: absolute;
        top: 10px;
        right: 10px;
        display: flex;
        gap: 8px;
    }

    .tags {
        margin-top: 10px;
        display: flex;
        flex-wrap: wrap;
        gap: 6px;
    }

    .tag {
        background: #555;
        border-radius: 4px;
        padding: 4px 8px;
        font-size: 12px;
        color: #f1f1f1;
    }

    button {
        font-size: 0.9rem;
    }

    /* Модальное окно */
    .custom-modal-overlay {
        position: fixed !important;
        inset: 0 !important;
        background-color: rgba(0, 0, 0, 0.75) !important;
        display: flex !important;
        justify-content: center !important;
        align-items: center !important;
        z-index: 9999 !important;
    }

    .custom-modal {
        background: #1e1e2f !important;
        color: #ffffff !important;
        padding: 30px !important;
        border-radius: 12px !important;
        width: 100% !important;
        max-width: 600px !important;
        box-shadow: 0 15px 40px rgba(0, 0, 0, 0.7) !important;
        animation: fadeIn 0.3s ease-in-out !important;
        z-index: 10000 !important;
    }

    @keyframes fadeIn {
        from {
            opacity: 0;
            transform: scale(0.95);
        }

        to {
            opacity: 1;
            transform: scale(1);
        }
    }

    .custom-modal .modal-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        border-bottom: 1px solid #444;
        margin-bottom: 20px;
    }

        .custom-modal .modal-header h2 {
            margin: 0;
            font-size: 1.5rem;
        }

    .custom-modal .close-btn {
        background: transparent;
        border: none;
        font-size: 1.5rem;
        color: #f1f1f1;
        cursor: pointer;
        transition: color 0.2s;
    }

        .custom-modal .close-btn:hover {
            color: #ff6b6b;
        }

    .custom-modal .input-field {
        background-color: #2e2e3e !important;
        color: #f1f1f1 !important;
        border: 1px solid #444 !important;
    }

    /* Убираем скролл внутри модалки */
    .custom-modal .modal-form {
        /* Удалены max-height и overflow: auto чтобы не было скролла */
    }

    .custom-modal .btn {
        width: auto;
    }

    .custom-modal .btn-success {
        background-color: #4caf50 !important;
        border-color: #4caf50 !important;
    }

        .custom-modal .btn-success:hover {
            background-color: #45a047 !important;
            border-color: #45a047 !important;
        }

    .custom-modal .btn-secondary {
        background-color: #6c757d !important;
        border-color: #6c757d !important;
    }

        .custom-modal .btn-secondary:hover {
            background-color: #5a6268 !important;
            border-color: #5a6268 !important;
        }

    .assigned-user {
        margin-top: 10px;
        font-size: 0.95em;
        color: #8de7e7;
    }

    .email {
        color: #80bfbf;
        font-size: 0.90em;
        margin-left: 3px;
    }
</style>
