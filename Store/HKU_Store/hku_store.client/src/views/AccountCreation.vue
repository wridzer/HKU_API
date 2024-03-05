<template>
    <div class="register">
        <h1>Create Account</h1>
        <form @submit.prevent="register">
            <div class="form-group">
                <label for="name">Name:</label>
                <input type="text" id="name" v-model="user.name" required>
            </div>
            <div class="form-group">
                <label for="email">Email:</label>
                <input type="email" id="email" v-model="user.email" required>
            </div>
            <div class="form-group">
                <label for="password">Password:</label>
                <input type="password" id="password" v-model="user.password" required>
            </div>
            <button type="submit">Create Account</button>
        </form>
    </div>
</template>

<script setup>import { ref } from 'vue';

const user = ref({
  name: '',
  email: '',
  password: '',
});

function register() {
  // Hier zou je registratielogica implementeren.
  // Dit kan een API-aanroep zijn naar je server die op zijn beurt OpenID of een andere dienst gebruikt.
  console.log('Registering:', user.value);
  // Normaal zou je hier feedback geven aan de gebruiker, bijvoorbeeld door een succesbericht te tonen of door te navigeren naar een inlogpagina.
}</script>

<style scoped>
    .register {
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
