<template>
    <div class="form-container">
        <h2>Register</h2>
        <form @submit.prevent="register" class="register-form">
            <div class="form-group">
                <label for="username">Username:</label>
                <input id="username" v-model="username" required>
            </div>
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
                <input type="password" id="password" v-model="password" @input="validatePassword(password)" required>
            </div>
            <div class="form-group">
                <label for="birthDate">Birth Date:</label>
                <input type="date" value-type="DD/MM/YYYY" format="DD-MM-YYYY">
            </div>
            <button v-if="!loading" type="submit" class="submit-btn">Register</button>
            <div v-else><i>Loading...</i></div> <!-- Vervang dit door je laadicoon -->
        </form>
    </div>
    <div class="password-requirements">
        <p>Password must meet the following requirements:</p>
        <ul>
            <li v-for="requirement in passwordRequirements"
                :class="{ 'valid': requirement.valid }">
                {{ requirement.rule }}
            </li>
        </ul>
    </div>
</template>

<script setup>
    import { ref } from 'vue';
    import axios from 'axios';
    import { useRouter } from 'vue-router'

    const name = ref('');
    const email = ref('');
    const password = ref('');
    const birthDate = ref('');
    const username = ref('');
    const loading = ref(false);
    const router = useRouter()

    const register = async () => {
        loading.value = true;
        try {
            await axios.post('/api/users/register', {
                name: name.value,
                email: email.value,
                password: password.value,
                username: username.value,
            });
            alert('Registration successful!');
            router.push('/');
        } catch (error) {
            console.error(error);
            let errorMessage = 'Registration failed: ';

            if (error.response && error.response.data) {
                const errors = error.response.data;
                Object.keys(errors).forEach(key => {
                    errors[key].forEach(msg => errorMessage += msg + ' ');
                });
            } else {
                errorMessage += 'Unknown error';
            }

            alert(errorMessage.trim());
        } finally {
            loading.value = false;
        }
    };
</script>
<script>
    export default {
        data() {
            return {
                password: '',
                passwordRequirements: [
                    { rule: 'At least 8 characters long', valid: false },
                    { rule: 'Includes at least one uppercase letter', valid: false },
                    { rule: 'Includes at least one lowercase letter', valid: false },
                    { rule: 'Includes at least one digit', valid: false },
                    { rule: 'Includes at least one special character (@#$%^&*)', valid: false },
                ],
                loading: false,
            };
        },
        watch: {
            password(newVal) {
                this.validatePassword(newVal);
            },
        },
        methods: {
            validatePassword(password) {
                this.passwordRequirements[0].valid = password.length >= 8;
                this.passwordRequirements[1].valid = /[A-Z]/.test(password);
                this.passwordRequirements[2].valid = /[a-z]/.test(password);
                this.passwordRequirements[3].valid = /\d/.test(password);
                this.passwordRequirements[4].valid = /[@#$%^&*]/.test(password);
            },
        },
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

    .valid {
        color: green;
    }

    li:not(.valid) {
        color: red;
    }

    password-requirements {
        display: block;
        margin-bottom: 5px;
    }

</style>
