<template>
    <div>
        <h1>Update Profile</h1>
        <form @submit.prevent="updateProfile">
            <input v-model="form.username" placeholder="Username" required>
            <input v-model="form.email" type="email" placeholder="Email" required>
            <button type="submit">Update Profile</button>
        </form>
    </div>
</template>

<script>
    import axios from 'axios';
    import { useRouter } from 'vue-router'
       
    const router = useRouter()

    export default {
      data() {
        return {
          form: {
            username: '',
            email: ''
          }
        };
      },
      methods: {
        async updateProfile() {
          try {
            const response = await axios.post('/api/users/updateprofile', this.form);
            alert('Profile updated successfully');
            router.push('/');
          } catch (error) {
            console.error('Failed to update profile:', error.response.data);
          }
        }
      }
    }

</script>
