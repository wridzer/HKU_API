<template>
    <div>
        <h2>Gebruikerslijst</h2>
        <ul>
            <li v-for="user in users" :key="user.id">
                {{ user.name }} - {{ user.email }}
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
                console.error(response, response.text);
                users.value = await response.json();
            });

            return { users };
        }
    }
</script>