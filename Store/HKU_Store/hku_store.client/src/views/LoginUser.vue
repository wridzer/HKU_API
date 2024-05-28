<template>
    <div>
        <h1>Login</h1>
        <form @submit.prevent="login" id="login-form">
            <div class="form-group">
                <label for="email">Email:</label>
                <input type="email" v-model="email" id="email" class="form-control" required>
            </div>
            <div class="form-group">
                <label for="password">Password:</label>
                <input type="password" v-model="password" id="password" class="form-control" required>
            </div>
            <button type="submit" class="btn btn-primary">Login</button>
        </form>
        <div v-if="error" class="error-message">{{ error }}</div>
    </div>
</template>

<script lang="ts">
    import { defineComponent } from 'vue';
    import axios from 'axios';

    export default defineComponent({
        data() {
            return {
                email: '',
                password: '',
                error: ''
            };
        },
        methods: {
            async login() {
                try {
                    const response = await axios.post('/api/users/login', {
                        email: this.email,
                        password: this.password
                    });

                    if (response.data.status === 'success') {
                        const userId = response.data.userId;
                        // Check if the DLL callback is needed
                        const urlParams = new URLSearchParams(window.location.search);
                        if (urlParams.has('dll') && urlParams.get('dll') === 'true') {
                            window.location.href = `http://localhost:8080/callback?status=success&user_id=${userId}`;
                        } else {
                            // Normal login process
                            window.location.href = '/';
                        }
                    } else {
                        this.error = 'Invalid login attempt.';
                    }
                } catch (err) {
                    this.error = 'An error occurred during login.';
                }
            }
        }
    });
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
