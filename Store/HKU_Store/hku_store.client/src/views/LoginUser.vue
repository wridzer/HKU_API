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

<script setup>
    import { ref } from 'vue';
    import axios from 'axios';
    import { useRouter } from 'vue-router'

    const email = ref('');
    const password = ref('');
    const router = useRouter();

    const login = async () => {
        try {
            const response = await axios.post('/api/users/login', {
                email: email.value,
                password: password.value,
            });

            if (response.data.status === 'success') {
                alert('Login successful!');
                const userId = response.data.userId;
                // Check if the DLL callback is needed
                const urlParams = new URLSearchParams(window.location.search);
                if (urlParams.has('dll') && urlParams.get('dll') === 'true') {
                    window.location.href = `http://localhost:8080/callback?status=success&user_id=${userId}`;
                } else {
                    // Normal login process
                    router.push('/');
                }
            } else {
                this.error = 'Invalid login attempt.';
            }
        } catch (error) {
            console.error(error);
            let errorMessage = 'Login failed: ';

            if (error.response && error.response.data) {
                const errors = error.response.data;
                Object.keys(errors).forEach(key => {
                    errors[key].forEach(msg => errorMessage += msg + ' ');
                });
            } else {
                errorMessage += 'Unknown error';
            }

            alert(errorMessage.trim());
        }
    };
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
