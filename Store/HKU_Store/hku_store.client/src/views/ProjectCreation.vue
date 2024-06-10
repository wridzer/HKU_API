<template>
    <div>
        <label>
            <p>Project Name</p>
            <input type="text" v-model="project.Name" placeholder="Project Name" />
        </label>
        <label>
            <p>Project Description</p>
            <textarea v-model="project.Description" placeholder="Project Description"></textarea>
        </label>
        <label>
            <p>Project Image</p>
            <input type="file" @change="handleFileUpload" />
        </label>
        <label>
            <p>Add Contributors</p>
            <input type="text" v-model="newContributorUsername" placeholder="Add Contributor by Username" @keyup.enter="addContributorByUsername" />
        </label>
        <label>
            <button @click="createProject">Create Project</button>
        </label>
    </div>
</template>

<script>
    import axios from 'axios';

    export default {
        created() {
            this.fetchCurrentUser();
        },
        data() {
            return {
                project: {
                    Name: '',
                    Description: '',
                    Image: '',
                    Contributors: []
                },
                newContributorUsername: ''
            };
        },
        methods: {
            fetchCurrentUser() {
                axios.get('/api/users/currentuser')
                    .then(response => {
                        if (response.data) {
                            this.project.Contributors.push(response.data.id);
                        } else {
                            console.error('No user ID returned from the API.');
                        }
                    })
                    .catch(error => {
                        console.error("Failed to fetch current user:", error);
                    });
            },
            handleFileUpload(event) {
                const file = event.target.files[0];
                const reader = new FileReader();
                reader.onload = (e) => {
                    this.project.Image = e.target.result;
                };
                reader.readAsDataURL(file);
            },
            addContributorByUsername() {
                axios.get(`/api/users/find?username=${this.newContributorUsername}`)
                    .then(response => {
                        this.project.Contributors.push(response.data.userId);
                        this.newContributorUsername = ''; // Clear input after adding
                    })
                    .catch(error => console.error('Error adding contributor:', error));
            },
            async createProject() {
                try {
                    const response = await fetch('/api/projects', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(this.project)
                    });
                    if (!response.ok) {
                        throw new Error('Failed to create project');
                    }
                    const data = await response.json();
                    // Navigate to the project management page
                    this.$router.push({ name: 'ProjectManagement', params: { id: data.id } });
                } catch (error) {
                    console.error('Error creating project:', error);
                }
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

    input {
        border: 1px solid #ccc;
        border-radius: 4px;
        padding: 10px 15px;
        transition: border-color 0.3s;
        margin-left: 15px;
    }

    textarea {
        border: 1px solid #ccc;
        border-radius: 4px;
        padding: 10px 15px;
        width: 50%;
        transition: border-color 0.3s;
        margin-left: 15px;
    }

    textarea {
        border-color: #0056b3; /* Highlight focus state */
    }

    label {
        display: block;
        margin-bottom: 10px;
    }
</style>
