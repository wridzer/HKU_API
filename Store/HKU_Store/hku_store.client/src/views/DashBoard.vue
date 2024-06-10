<template>
    <div>
        <h1>Dashboard</h1>
        <button v-if="isLoggedIn" @click="goToCreateProject">Create New Project</button>
        <div v-if="isLoggedIn && projects.length">
            <h2>Your Projects</h2>
            <ul>
                <li v-for="project in projects" :key="project.id">
                    <div class="project_list_item">
                        <div>{{ project.name }}</div>
                        <button @click="() => editProject(project.id)">Edit</button>
                    </div>
                </li>
            </ul>
        </div>
        <div v-else-if="!isLoggedIn">
            <p>You are not logged in. Please go to "Account" and create an account or log in.</p>
        </div>
        <div v-else>
            <p>You have no projects. Click above to create one.</p>
        </div>
    </div>
</template>

<script>
    import { ref, onMounted } from 'vue';
    import axios from 'axios';
    import { useRouter } from 'vue-router';


    export default {
        setup() {
            const projects = ref([]);
            const userId = ref('');
            const isLoggedIn = ref(false); // This should be dynamically set based on auth status
            const router = useRouter();

            const checkLoginStatus = async () => {
                try {
                    const response = await axios.get('/api/users/currentuser')
                    if (response.data) {
                        isLoggedIn.value = true
                        userId.value = response.data.id
                        fetchProjects()
                    } else {
                        isLoggedIn.value = false
                    }
                } catch (error) {
                    isLoggedIn.value = false
                    console.error('Error fetching current user:', error)
                }
            }

            const fetchProjects = async () => {
                try {
                    const response = await axios.get(`/api/projects/user/${userId.value}`);
                    projects.value = response.data;
                } catch (error) {
                    console.error('Failed to fetch projects for user:', userId.value, error);
                }
            };

            onMounted(() => {
                checkLoginStatus();
                if (isLoggedIn.value) {
                    fetchProjects();
                }
            });

            const goToCreateProject = () => {
                router.push('/create-project')
            };

            const editProject = (projectId) => {
                router.push('/manage-project/' + projectId)
            };

            return { projects, isLoggedIn, goToCreateProject, editProject };
        }
    }
</script>

<style scoped>
    .form-container {
        max-width: 500px;
        margin: 0 auto;
        padding: 20px;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        border-radius: 8px;
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

    .button {
        padding: 10px 20px;
        background-color: #007bff;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
    }

    .button:hover {
        background-color: #0056b3;
    }

    @media (max-width: 768px) {
        .form-container {
            padding: 10px;
        }

        .button {
            padding: 10px;
        }
    }

    ul {
        list-style-type: none;
        padding: 0; 
        margin: 0; 
    }

    li {
        margin-bottom: 10px;
        background-color: #3e3e3e;
        padding: 10px;
        border-radius: 5px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        width: 100%;
    }

    .project_list_item {
        display: flex; 
        align-items: center; 
        justify-content: space-between; 
    }

    .project_list_item > div {
        flex: 1; 
        margin-right: 10px; /* Voegt ruimte toe tussen tekst en knop */
        overflow: hidden; /* Zorgt ervoor dat de tekst niet buiten de div vloeit */
        white-space: nowrap; /* Voorkomt tekstomloop naar de volgende regel */
        text-overflow: ellipsis; /* Voegt ellipsis toe (...) als de tekst overloopt */
    }   

    button {
        padding: 5px 15px;
        background-color: #fe1f4c; 
        color: white; 
        border: none; 
        border-radius: 5px; 
        cursor: pointer; 
        transition: background-color 0.3s; 
    }

    button:hover {
        background-color: #b11535; 
    }

</style>
