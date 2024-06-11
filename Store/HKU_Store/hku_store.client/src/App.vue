<script setup>
    import { ref } from 'vue';
    import Sidebar from './views/Sidebar.vue';

    const sidebar = ref(null);
</script>

<template>
    <div id="app">
        <Sidebar />
        <div class="content">
            <router-view />
        </div>
    </div>
</template>

<style scoped>
header {
  line-height: 1.5;
}

.logo {
  display: block;
  margin: 0 auto 2rem;
}

#app {
    display: flex;
    height: 100vh; /* Volledige hoogte van het viewport */
}

.sidebar {
    width: 200px; /* Of een andere vaste breedte */
    background-color: #f4f4f4; /* Achtergrondkleur van de sidebar */
}

    .content {
        flex-grow: 1; /* Takes up all available space */
        overflow: auto; /* Adds scroll if needed */
        padding: 20px;
        padding-top: 100px;
    }

html, body {
    margin: 0;
    padding: 0;
    width: 100%;
    height: 100%;
    overflow: hidden; /* Prevents scrollbars if not needed */
}

@media (min-width: 1024px) {
  header {
    display: flex;
    place-items: center;
    padding-right: calc(var(--section-gap) / 2);
  }

  .logo {
    margin: 0 2rem 0 0;
  }

  header .wrapper {
    display: flex;
    place-items: flex-start;
    flex-wrap: wrap;
  }

}
</style>
