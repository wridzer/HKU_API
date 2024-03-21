<template>
    <div class="form-container">
        <h2>Register</h2>
        <form @submit.prevent="register" class="register-form">
            <div class="form-group">
                <label for="name">Name:</label>
                <input id="name" v-model="name" required>
            </div>
            <div class="form-group">
                <label for="email">Email:</label>
                <input type="email" id="email" v-model="email" required>
            </div>
            <div class="form-group">
                <label for="password">Password:</label>
                <input type="password" id="password" v-model="password" required>
            </div>
            <div class="form-group">
                <label for="birthDate">Birth Date:</label>
                <input type="date" id="birthDate" v-model="birthDate" required>
            </div>
            <div class="form-group">
                <label for="username">Username:</label>
                <input id="username" v-model="username" required>
            </div>
            <button type="submit" class="submit-btn">Register</button>
        </form>
    </div>
</template>

<script setup>
    import { ref } from 'vue';
    import axios from 'axios';

    const name = ref('');
    const email = ref('');
    const password = ref('');
    const birthDate = ref('');
    const username = ref('');

    const register = async () => {
        try {
            await axios.post('/api/users/register', {
                name: name.value,
                email: email.value,
                password: password.value,
                username: username.value,
            });
            alert('Registration successful!');
        } catch (error) {
            console.error(error);
            alert('Registration failed: ' + error.response.data.ToString());
        }
    };
</script>


<style scoped>
.form-container {
    max-width: 500px;
    margin: 0 auto;
    padding: 20px;
    box-shadow: 0 4px 6px rgba(0,0,0,0.1);
    border-radius: 8px;
}

.register-form {
    display: flex;
    flex-direction: column;
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
    padding: 10px;
    border: 1px solid #ccc;
    border-radius: 4px;
}

.submit-btn {
    padding: 10px 20px;
    background-color: #007bff;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
}

.submit-btn:hover {
    background-color: #0056b3;
}

@media (max-width: 768px) {
    .form-container {
        padding: 10px;
    }

    .submit-btn {
        padding: 10px;
    }
}
</style>
