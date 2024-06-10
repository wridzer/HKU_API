<template>
    <div class="store">
        <div class="project" v-for="project in projects" :key="project.id">
            <img :src="project.image" :alt="project.Name" class="project-image">
            <h2>{{ project.name }}</h2>
            <p>{{ project.description }}</p>
            <button @click="navigateToProjectLeaderboards(project.id, project.name)">View Leaderboards</button>
        </div>
    </div>
</template>

<script>
    export default {
        data() {
            return {
                projects: []
            };
        },
        async created() {
            try {
                const response = await fetch('/api/projects');
                this.projects = await response.json();
            } catch (error) {
                console.error('Error fetching projects:', error);
            }
        },
        methods: {
            navigateToProjectLeaderboards(projectId, projectName) {
                this.$router.push({ name: 'LeaderboardPage', params: { projectId, projectName } });
            }
        }
    };
</script>

<style>
    .store {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
        gap: 20px;
        padding: 20px;
        max-width: calc(100% - 200px);
        width: 1000px;
    }

    .project {
        border: 1px solid #ccc;
        border-color: #fe1f4c;
        border-radius: 8px;
        overflow: hidden;
        padding: 10px;
    }

    .project-image {
        width: 100%;
        height: 200px;
        object-fit: contain;
    }

    h2, p {
        padding: 0 15px;
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

</style>
