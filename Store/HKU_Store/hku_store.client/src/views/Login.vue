<template>
    <div class="login">
        <h1>Login</h1>
        <form @submit.prevent="login">
            <div class="form-group">
                <label for="email">Email:</label>
                <input type="email" id="email" v-model="credentials.email" required>
            </div>
            <div class="form-group">
                <label for="password">Password:</label>
                <input type="password" id="password" v-model="credentials.password" required>
            </div>
            <button type="submit">Log In</button>
        </form>
    </div>
</template>

<script setup>
    import { ref } from 'vue';

    const credentials = ref({
        email: '',
        password: '',
    });

    function login() {
        // Voeg hier je inloglogica toe
        console.log('Logging in with:', credentials.value);
        // Normaal zou je hier een API call doen om de gebruiker te authenticeren
    }
</script>

<style scoped>
    .login {
        max-width: 300px;
        margin: auto;
        padding: 20px;
        border: 1px solid #ccc;
        border-radius: 5px;
    }

    .form-group {
        margin-bottom: 20px;
    }

        .form-group label {
            display: block;
            margin-bottom: 5px;
        }

        .form-group input {
            width: 100%;
            padding: 8px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }

    button {
        width: 100%;
        padding: 10px;
        border: none;
        background-color: #007bff;
        color: white;
        cursor: pointer;
        border-radius: 4px;
    }

        button:hover {
            background-color: #0056b3;
        }
</style>
