<template>
    <div class="home">
        <h1>Welcome to Our App</h1>
        <p v-if="isLoggedIn">You are logged in!</p>
        <p v-else>You are not logged in.</p>
        <button v-if="!isLoggedIn" @click="goToLogin">Log In</button>
        <button v-if="!isLoggedIn" @click="goToRegister">Create Account</button>
    </div>
</template>

<script setup>
    import { ref } from 'vue'
    import { useRouter } from 'vue-router'

    // Een gesimuleerde manier om de inlogstatus van een gebruiker te controleren
    const isLoggedIn = ref(false)

    const router = useRouter()

    function goToLogin() {
        router.push('/login')
    }

    function goToRegister() {
        router.push('/AccountCreation')
    }
</script>

<style scoped>
    .home {
        text-align: center;
        padding: 20px;
    }

    button {
        margin: 10px;
        padding: 10px 20px;
        cursor: pointer;
    }
</style>
