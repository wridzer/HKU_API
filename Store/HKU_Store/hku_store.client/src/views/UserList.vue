<template>
    <div class="user-list-container">
        <h2>Gebruikerslijst</h2>
        <ul>
            <li v-for="user in users" :key="user.id">
                {{ user.userName }} - {{ user.email }}
            </li>
        </ul>
    </div>
</template>

<script>
import { ref, onMounted } from 'vue';

    export default {
        setup() {
            const users = ref([]);

            onMounted(async () => {
                const response = await fetch('/api/users');
                users.value = await response.json();
            });

            return { users };
        }
    }
</script>

<style scoped>
    .user-list-container ul {
        width: 100%; 
        max-width: 800px; 
        margin: auto;
    }
</style>