<template>
    <div class="chat-container">
        <h1>Командный чат</h1>

        <div class="chat-box">
            <div class="message" v-for="msg in messages" :key="msg.id">
                <strong>{{ msg.senderName }}:</strong> {{ msg.content }}
            </div>
        </div>

        <form @submit.prevent="sendMessage" class="chat-form">
            <input type="text" v-model="message" placeholder="Введите сообщение..." required />
            <button type="submit">Отправить</button>
        </form>
    </div>
</template>

<script>
import { onMounted, ref } from 'vue';
import { auth } from '@/store/authStore';
import { createConnection } from '@/signalr';

export default {
  setup() {
    const messages = ref([]);
    const message = ref('');
    let connection;

    onMounted(async () => {
      connection = createConnection(auth.token);

      connection.on('ReceiveMessage', (user, content) => {
        messages.value.push({ senderName: user, content });
      });

      try {
        await connection.start();
        console.log('SignalR connected');
      } catch (e) {
        console.error('Connection failed:', e);
      }
    });

    const sendMessage = async () => {
      if (!message.value.trim()) return;
      try {
        await connection.invoke('SendMessage', 'Вы', message.value);
        message.value = '';
      } catch (e) {
        console.error('Ошибка при отправке:', e);
      }
    };

    return { messages, message, sendMessage };
  },
};
</script>

<style scoped>
    .chat-container {
        max-width: 600px;
        margin: 60px auto;
        padding: 30px;
        background: #1e1e2f;
        border-radius: 8px;
        color: white;
    }

    .chat-box {
        max-height: 300px;
        overflow-y: auto;
        background: #2e2e3e;
        padding: 15px;
        border-radius: 4px;
        margin-bottom: 15px;
    }

    .message {
        margin-bottom: 10px;
    }

    .chat-form {
        display: flex;
        gap: 10px;
    }

    input {
        flex: 1;
        padding: 8px;
        border: none;
        border-radius: 4px;
        background: #3e3e50;
        color: white;
    }

    button {
        padding: 8px 16px;
        background: #4a90e2;
        border: none;
        border-radius: 4px;
        color: white;
        cursor: pointer;
    }
</style>
