<template>
    <div>
        <h1>Account settings</h1>
        <!-- Show logout button if the user is logged in -->
        <button v-if="isLoggedIn" @click="logout">Logout</button>
        <button v-if="isLoggedIn" @click="EditProfile">Edit profile</button>
        <!-- Show login and account creation buttons if the user is not logged in -->
        <div v-else>
            <button @click="goToLogin">Login</button>
            <button @click="goToRegister">Create Account</button>
        </div>
    </div>
</template>

<script setup>
    import { ref, onMounted } from 'vue'
    import { useRouter } from 'vue-router'
    import UsersList from './UserList.vue'
    import axios from 'axios'

    // Een gesimuleerde manier om de inlogstatus van een gebruiker te controleren
    const isLoggedIn = ref(false)
    const userName = ref('')

    const router = useRouter()

    const checkLoginStatus = async () => {
        try {
            const response = await axios.get('/api/users/currentuser')
            if (response.data) {
                isLoggedIn.value = true
                userName.value = response.data.userName
            } else {
                isLoggedIn.value = false
            }
        } catch (error) {
            isLoggedIn.value = false
            console.error('Error fetching current user:', error)
        }
    }

    onMounted(checkLoginStatus)

    const logout = async () => {
      try {
        const response = await axios.post('/api/users/logout');
        alert('Logged out successfully');
        checkLoginStatus();
        router.push('/');
      } catch (error) {
        console.error('Logout failed:', error.response.data);
      }
    }

    function EditProfile() {
        router.push('/UpdateProfile')
    }

    function goToLogin() {
        router.push('/LoginUser')
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

    #app {
        max-width: 90%; 
        margin: auto; 
    }
</style>
