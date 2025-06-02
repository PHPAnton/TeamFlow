<template>
    <div class="modal-backdrop1">
        <div class="modal1">
            <button class="close-btn" @click="$emit('close')">&times;</button>
            <h2>Участники проекта</h2>

            <!-- Выбор проекта -->
            <label for="project-select"><b>Выберите проект:</b></label>
            <select v-model="selectedProjectId" id="project-select" class="form-select mb-3">
                <option disabled value="">Выберите проект</option>
                <option v-for="project in projects" :key="project.id" :value="project.id">
                    {{ project.title }}
                </option>
            </select>

            <!-- Добавить участника -->
            <div v-if="isOwner && selectedProjectId" class="add-member-form">
                <h4>Добавить участника</h4>
                <form @submit.prevent="addMember">
                    <input v-model="newMemberEmail" placeholder="Email участника" class="form-control mb-2" required />
                    <select v-model="newMemberRole" class="form-select mb-2" required>
                        <option value="Member">Участник</option>
                        <option value="Admin">Администратор</option>
                    </select>
                    <button class="btn btn-success w-100" :disabled="addingMember">Добавить</button>
                </form>
                <div v-if="addError" class="text-danger mt-1">{{ addError }}</div>
                <div v-if="addSuccess" class="text-success mt-1">Участник приглашён!</div>
            </div>

            <!-- Список участников -->
            <div v-if="selectedProjectId && !membersLoading">
                <h4>Участники:</h4>
                <ul class="list-group">
                    <!-- Владелец всегда сверху -->
                    <li v-if="owner" class="list-group-item d-flex justify-content-between align-items-center owner">
                        <div>
                            <b>{{ owner.Username }}</b>
                            <span class="badge bg-primary ms-2">Владелец</span>
                            <span class="text-muted ms-1">({{ owner.Email }})</span>
                        </div>
                        <span class="badge bg-secondary">{{ owner.Role }}</span>
                    </li>
                    <!-- Остальные участники -->
                    <li v-for="member in nonOwnerMembers" :key="member.Id"
                        class="list-group-item d-flex justify-content-between align-items-center">
                        <div>
                            {{ member.Username }} <span class="text-muted">({{ member.Email }})</span>
                        </div>
                        <div class="d-flex align-items-center">
                            <template v-if="isOwner">
                                <select v-model="member.Role"
                                        @change="changeRole(member)"
                                        class="form-select form-select-sm me-2" style="width: 120px;">
                                    <option value="Member">Участник</option>
                                    <option value="Admin">Администратор</option>
                                </select>
                                <button @click="removeMember(member)" class="btn btn-outline-danger btn-sm">&times;</button>
                            </template>
                            <span v-else class="badge bg-secondary">{{ member.Role }}</span>
                        </div>
                    </li>
                </ul>
                <div v-if="members.length === 0" class="text-muted">В этом проекте пока нет участников.</div>
            </div>
            <div v-if="selectedProjectId && membersLoading" class="text-muted">Загрузка участников...</div>
            <div v-if="!selectedProjectId" class="text-muted">Сначала выберите проект</div>
        </div>
    </div>
</template>

<script setup>
    import { ref, computed, watch } from 'vue'

    const props = defineProps({
        projects: Array,
        currentUserId: String, // строка (guid) !!!
        projectOwnerId: String // строка (guid) !!!
    })

    const emit = defineEmits(['close'])

    // Состояния
    const selectedProjectId = ref('')
    const members = ref([])
    const membersLoading = ref(false)
    const newMemberEmail = ref('')
    const newMemberRole = ref('Member')
    const addingMember = ref(false)
    const addError = ref('')
    const addSuccess = ref(false)

    watch(selectedProjectId, async (newId) => {
        if (!newId) {
            members.value = []
            return
        }
        await loadMembers(newId)
    })

    async function loadMembers(projectId) {
        membersLoading.value = true
        try {
            const token = localStorage.getItem('token')
            const response = await fetch(`/api/ProjectMembers/${projectId}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            })
            if (!response.ok) throw new Error('Ошибка загрузки участников')
            members.value = await response.json()
        } catch (e) {
            members.value = []
            alert('Ошибка при загрузке участников')
        } finally {
            membersLoading.value = false
        }
    }

    // === Добавить участника ===
    async function addMember() {
        addError.value = ''
        addSuccess.value = false
        addingMember.value = true
        try {
            const token = localStorage.getItem('token')
            const response = await fetch(`/api/ProjectMembers/${selectedProjectId.value}/add`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    email: newMemberEmail.value,
                    role: newMemberRole.value
                })
            })
            if (!response.ok) {
                const err = await response.json()
                addError.value = err || 'Ошибка при добавлении'
            } else {
                addSuccess.value = true
                newMemberEmail.value = ''
                newMemberRole.value = 'Member'
                await loadMembers(selectedProjectId.value)
                setTimeout(() => addSuccess.value = false, 2000)
            }
        } catch (e) {
            addError.value = 'Ошибка при добавлении'
        }
        addingMember.value = false
    }

    // === Поменять роль участника ===
    async function changeRole(member) {
        const token = localStorage.getItem('token')
        try {
            await fetch(`/api/ProjectMembers/${member.Id}/role`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ role: member.Role })
            })
            // Можно добавить всплывающее сообщение об успехе
        } catch (e) {
            alert('Ошибка при смене роли')
        }
    }

    // === Удалить участника ===
    async function removeMember(member) {
        if (!confirm('Удалить этого участника из проекта?')) return
        const token = localStorage.getItem('token')
        try {
            await fetch(`/api/ProjectMembers/${member.Id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            })
            await loadMembers(selectedProjectId.value)
        } catch (e) {
            alert('Ошибка при удалении участника')
        }
    }

    // === Сортировки и права ===
    // Определяем владельца проекта (Owner)
    const owner = computed(() =>
        members.value.find(m => m.UserId === props.projectOwnerId)
    )

    // Остальные участники (не owner)
    const nonOwnerMembers = computed(() =>
        members.value.filter(m => m.UserId !== props.projectOwnerId)
    )

    // Владелец модалки?
    const isOwner = computed(() =>
        props.currentUserId === props.projectOwnerId
    )
</script>

<style scoped>
    .modal-backdrop1 {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: #292C41cc;
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 1000;
    }

    .modal1 {
        background: #373a59;
        border-radius: 12px;
        padding: 32px 24px 24px 24px;
        min-width: 340px;
        box-shadow: 0 8px 32px rgba(0,0,0,0.2);
        position: relative;
        color: #fff;
    }

    .close-btn {
        position: absolute;
        right: 18px;
        top: 12px;
        font-size: 26px;
        background: none;
        border: none;
        color: #fff;
        cursor: pointer;
    }

    .owner {
        background: #222247;
        color: #fff;
    }

    .list-group {
        margin-bottom: 8px;
    }

    .list-group-item {
        background: #343556;
        border: none;
        color: #fff;
        margin-bottom: 2px;
    }

    .form-control, .form-select {
        background: #292C41;
        color: #fff;
        border: 1px solid #556;
    }

        .form-control:focus, .form-select:focus {
            border-color: #6cf;
            outline: none;
        }

    .text-muted {
        color: #bfc2da !important;
    }

    .text-danger {
        color: #fc7b7b;
    }

    .text-success {
        color: #67ff9c;
    }

    .add-member-form {
        margin-bottom: 24px;
        background: #232340;
        border-radius: 7px;
        padding: 12px;
    }
</style>
