<template>
    <div class="store">
        <div class="project" v-for="project in projects" :key="project.ID">
            <img :src="project.image" :alt="project.Name" class="project-image">
            <h2>{{ project.name }}</h2>
            <p>{{ project.description }}</p>
        </div>
    </div>
</template>

<script setup>import { ref, onMounted } from 'vue';
import axios from 'axios';

const projects = ref([]);

onMounted(async () => {
  try {
    const response = await axios.get('/api/projects');
    projects.value = response.data;
  } catch (error) {
    console.error('Failed to fetch projects:', error);
  }
});</script>

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

</style>
