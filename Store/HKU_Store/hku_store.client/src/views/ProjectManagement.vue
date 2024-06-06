<template>
    <div class="main-content">
        <h1>Edit Project</h1>
        <!-- Input for project name -->
        <div>
            <label for="projectId">Project ID:</label>
            <p>{{ $route.params.id }}</p>
        </div>
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

        <div>
            <h2>Leaderboards</h2>
            <!-- Create New Leaderboard -->
            <div>
                <h3>Create New Leaderboard</h3>
                <ul>
                    <li>
                        <input v-model="newLeaderboard.name" type="text" placeholder="Leaderboard Name" />
                    </li>
                    <li >
                        <input v-model="newLeaderboard.description" type="text" placeholder="Description" />
                    </li>
                    <li>
                        <select v-model="newLeaderboard.sortMethod">
                            <option value="None">None</option>
                            <option value="Ascending">Ascending</option>
                            <option value="Descending">Descending</option>
                        </select>
                    </li>
                    <li>
                        <select v-model="newLeaderboard.displayType">
                            <option value="None">None</option>
                            <option value="Seconds">Seconds</option>
                            <option value="Milliseconds">Milliseconds</option>
                            <option value="Numeric">Numeric</option>
                        </select>
                    </li>
                </ul>
                <button @click="createLeaderboard">Create Leaderboard</button>
            </div>

            <!-- List Existing Leaderboards -->
            <h2>Existing Leaderboards</h2>
            <ul>
                <li v-for="leaderboard in leaderboards" :key="leaderboard.id">
                    <div class="leaderboard_list_item">
                        <input v-model="leaderboard.name" placeholder="Leaderboard Name" />
                        <input v-model="leaderboard.description" placeholder="Description" />
                        <button @click="updateLeaderboard(leaderboard.id)">Update</button>
                        <button @click="deleteLeaderboard(leaderboard.id)">Delete</button>
                        <p>{{ leaderboard.id }}</p>
                    </div>
                </li>
            </ul>
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
                newContributor: '',
                leaderboards: [], // Array to store leaderboards
                newLeaderboard: {  // Model for creating new leaderboards
                    projectId: this.$route.params.id,
                    name: '',
                    description: '',
                    sortMethod: 'None',
                    displayType: 'None'
                }
            };
        },
        created() {
            this.fetchProject();
            this.fetchLeaderboards();
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
            },
            fetchLeaderboards() {
                axios.get(`/api/leaderboards/by-project/${this.project.id}`)
                    .then(response => {
                        this.leaderboards = response.data;
                    })
                    .catch(error => console.error('Failed to fetch leaderboards:', error));
            },
            createLeaderboard() {
                axios.post(`/api/leaderboards/create`, this.newLeaderboard)
                    .then(() => {
                        this.fetchLeaderboards(); // Refresh the leaderboard list
                    })
                    .catch(error => console.error('Failed to create leaderboard:', error));
            },
            updateLeaderboard(id) {
                const leaderboard = this.leaderboards.find(l => l.id === id);
                axios.put(`/api/leaderboards/update/${id}`, leaderboard)
                    .then(() => {
                        this.fetchLeaderboards(); // Refresh the leaderboard list
                    })
                    .catch(error => console.error('Failed to update leaderboard:', error));
            },
            deleteLeaderboard(id) {
                axios.delete(`/api/leaderboards/delete/${id}`)
                    .then(() => {
                        this.fetchLeaderboards(); // Refresh the leaderboard list
                    })
                    .catch(error => console.error('Failed to delete leaderboard:', error));
            },
        }
    };
</script>

<style scoped>

    .main-content {
        display: flow;
        justify-content: left; /* Center content horizontally */
        max-width: calc(100% - 200px); /* Full width minus sidebar width */
        min-width: 1200px; /* Resets minimum width to enable shrinking */
        padding: 20px; /* Provides some internal spacing */
        margin-top: 0;
        padding-top: 0;
    }

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

    @media (max-width: 768px) {
        .main-content {
            margin-left: 0; /* Removes margin to use full width on smaller screens */
            padding: 10px; /* Adjust padding for smaller screens */
        }
    }

    @media (min-width: 769px) and (max-width: 1024px) {
        .main-content {
            padding: 15px; /* Moderate padding for medium screens */
        }
    }

    div {
        padding: 10px;
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
        margin-left: 15px;
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
        width: 80%;
    }

    .leaderboard_list_item {
        display: flex;
        align-items: center;
        justify-content: space-between;
    }

    .leaderboard_list_item > div {
        flex: 1;
        margin-right: 10px; /* Voegt ruimte toe tussen tekst en knop */
        overflow: hidden; /* Zorgt ervoor dat de tekst niet buiten de div vloeit */
        white-space: nowrap; /* Voorkomt tekstomloop naar de volgende regel */
        text-overflow: ellipsis; /* Voegt ellipsis toe (...) als de tekst overloopt */
    }

    .button {
        padding: 5px 15px;
        margin: 15px;
        background-color: #fe1f4c;
        color: white;
        border: none;
        border-radius: 5px;
        cursor: pointer;
        transition: background-color 0.3s;
    }

    button {
        padding: 5px 15px;
        margin: 15px;
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

    input[type="text"], input[type="email"], input[type="password"] {
        border: 1px solid #ccc;
        border-radius: 4px;
        padding: 10px 15px;
        width: 50%;
        transition: border-color 0.3s;
        margin-left: 15px;
    }

    input[type="text"]:focus, input[type="email"]:focus, input[type="password"]:focus {
        border-color: #0056b3; /* Highlight focus state */
    }

    input{
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

    .select-container {
        position: relative;
        width: 100%; /* Full width for better control */
        margin: 10px 0; /* Spacing for clarity */
    }

    select {
        width: 50%; /* Full width of its container */
        padding: 10px 15px; /* Sufficient padding for touch targets */
        border-radius: 4px; /* Rounded corners */
        border: 1px solid #ccc; /* Subtle border */
        appearance: none; /* Remove default system styling */
        background-color: white; /* Background color */
        cursor: pointer; /* Cursor indication for clickable items */
        margin-left: 15px;
    }

    /* Custom arrow using a background image */
    select {
        background-image: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 4 5"><path fill="%23333" d="M2 0L0 2h4zm0 5L0 3h4z"/></svg>');
        background-repeat: no-repeat;
        background-position: right 10px center; /* Positioning the arrow nicely */
        background-size: 12px; /* Icon size */
    }

    /* Enhance the focus state for accessibility */
    select:focus {
        outline: none;
        border-color: #0056b3; /* Highlight color for focus */
        box-shadow: 0 0 0 2px rgba(0,86,179,0.25); /* Subtle glow effect */
    }

    input[type=file] {
        padding: 10px;
        border: 1px solid #ccc;
        border-radius: 4px;
        color: #333;
        font-size: 14px;
        background-color: #fff;
        cursor: pointer;
        transition: border-color 0.3s, box-shadow 0.3s;
    }

    input[type=file]:hover {
        border-color: #0056b3;
    }

    input[type=file]:focus  {
        outline: none;
        border-color: #0056b3;
        box-shadow: 0 0 5px rgba(0,86,179,0.5); /* Adding a light blue glow */
    }

    input[type=file]::file-selector-button {
          background-color: #007bff;
          color: white;
          border: none;
          padding: 10px 15px;
          border-radius: 4px;
          margin-right: 5px;
          cursor: pointer;
          transition: background-color 0.3s;
    }

    input[type=file]::file-selector-button:hover {
        background-color: #0056b3;
    }

</style>
