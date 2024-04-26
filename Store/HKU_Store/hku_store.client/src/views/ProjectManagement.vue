<template>
    <div>
        <h1>Edit Project</h1>
        <!-- Input for project name -->
        <div>
            <label for="projectName">Project Name:</label>
            <input id="projectName" v-model="project.name" type="text" placeholder="Enter project name">
        </div>

        <!-- Textarea for project description -->
        <div>
            <label for="projectDescription">Project Description:</label>
            <textarea id="projectDescription" v-model="project.description" placeholder="Enter project description"></textarea>
        </div>

        <!-- Input for updating project image -->
        <div>
            <label for="projectImage">Project Image:</label>
            <input id="projectImage" type="file" @change="handleImageChange" />
            <img :src="project.image" alt="Project Image" v-if="project.image" />
        </div>

        <!-- Input for adding contributors -->
        <div>
            <label for="contributorUsername">Add Contributor:</label>
            <input id="contributorUsername" v-model="newContributor" type="text" placeholder="Username" @keyup.enter="addContributor">
            <button @click="addContributor">Add</button>
        </div>

        <!-- Button to save changes -->
        <button @click="updateProject">Save Changes</button>

        <!-- Button to return to dashboard -->
        <button @click="gotoDashboard">Return to Dashboard</button>
    </div>
</template>

<script>
    import axios from 'axios';
    import router from '../router';

    export default {
        data() {
            return {
                project: {
                    id: this.$route.params.id,
                    name: '',
                    description: '',
                    image: '',
                    contributors: []
                },
                newContributor: ''
            };
        },
        created() {
            this.fetchProject();
        },
        methods: {
            fetchProject() {
                axios.get(`/api/projects/${this.project.id}`).then(response => {
                    this.project = { ...this.project, ...response.data };
                }).catch(error => {
                    console.error('Failed to fetch project details:', error);
                });
            },
            handleImageChange(event) {
                const file = event.target.files[0];
                const reader = new FileReader();
                reader.onload = (e) => {
                    this.project.image = e.target.result; // Assume base64 encoding
                };
                reader.readAsDataURL(file);
            },
            addContributor() {
                axios.get(`/api/users/by-username/${this.newContributor}`)
                    .then(response => {
                        this.project.contributors.push(response.data.userId); // Assuming the API returns the added contributor
                        this.newContributor = '';
                    })
                    .catch(error => {
                        console.error('Failed to add contributor:', error);
                    });
            },
            updateProject() {
                axios.put(`/api/projects/${this.project.id}`, this.project)
                    .then(() => {
                        alert('Project updated successfully!');
                        router.push('/dashboard');
                    })
                    .catch(error => {
                        console.error('Failed to update project:', error);
                    });
            },
            gotoDashboard() {
                router.push('/dashboard');
            }
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

    .valid {
        color: green;
    }

    li:not(.valid) {
        color: red;
    }

    .password-requirements {
        display: block;
        margin-bottom: 5px;
    }

    img {
        width: 100px; /* Example size */
    }
</style>
