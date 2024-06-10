<template>
    <div class="leaderboards-container">
        <h1>{{ projectName }} Leaderboards</h1>
        <div v-for="leaderboard in leaderboards" :key="leaderboard.id" class="leaderboard">
            <h2>{{ leaderboard.name }}</h2>
            <p class="description">{{ leaderboard.description }}</p>
            <ul class="entries-list">
                <li v-for="entry in leaderboard.entries" :key="entry.rank" class="entry-item">
                    <span class="rank">Rank: {{ entry.rank }}</span>
                    <span class="username">Player: {{ entry.username }}</span>
                    <span class="score">Score: {{ entry.score }}</span>
                </li>
            </ul>
        </div>
    </div>
</template>

<script>
    export default {
        data() {
            return {
                leaderboards: [],
                projectName: ''
            };
        },
        async created() {
            const projectId = this.$route.params.projectId;
            try {
                const projectResponse = await fetch(`/api/projects/${projectId}`);
                const projectData = await projectResponse.json();
                this.projectName = projectData.name;

                const response = await fetch(`/api/leaderboards/by-project/${projectId}`);
                const leaderboards = await response.json();

                for (const leaderboard of leaderboards) {
                    const entriesResponse = await fetch(`/api/leaderboards/entries/${leaderboard.id}?amount=20`);
                    const entriesData = await entriesResponse.json();
                    const entries = await Promise.all(entriesData.entries.map(async entry => {
                        const userResponse = await fetch(`/api/users/by-id/${entry.playerID}`);
                        const userData = await userResponse.json();
                        return {
                            ...entry,
                            username: userData.userName
                        };
                    }));
                    leaderboard.entries = entries;
                }
                this.leaderboards = leaderboards;
            } catch (error) {
                console.error('Error fetching project or leaderboards:', error);
            }
        }
    };
</script>

<style>
    .leaderboards-container {
        padding: 20px;
        font-family: Arial, sans-serif;
    }

    .leaderboard {
        gap: 20px;
        padding: 20px;
        border: 1px solid #ccc;
        border-color: #fe1f4c;
        border-radius: 8px;
        overflow: hidden;
        margin: 20px;
        max-width: calc(100% - 200px);
        width: 1000px;
    }

        .leaderboard h2 {
            margin-bottom: 10px;
        }

    .description {
        font-style: italic;
        color: #555;
        margin-bottom: 15px;
    }

    .entries-list {
        list-style-type: none;
        padding: 0;
    }

    .entry-item {
        display: flex;
        justify-content: space-between;
        padding: 10px;
        margin: 5px 0;
        border: 1px solid #ddd;
        border-radius: 4px;
        background-color: #fff;
    }

        .entry-item:nth-child(odd) {
            background-color: #f0f0f0;
        }

    .rank, .username, .score {
        margin-right: 10px;
        font-weight: bold;
    }

    .rank {
        color: #333;
    }

    .username {
        color: #007BFF;
    }

    .score {
        color: #28A745;
    }
</style>
