<template>
    <div class="sidebar">
        <img src="/favicon3.png" alt="Waterdamp Logo" width="150" height="150">
        <hr class="rounded">
        <ul>
            <li><router-link to="/dashboard">Project Dashboard</router-link></li>
            <li><router-link to="/storepage">Store</router-link></li>
            <li><router-link to="/guide">Guide</router-link></li>
            <li><router-link to="/downloadpage">Downloads</router-link></li>
            <li><router-link to="/">Account</router-link></li>
        </ul>
        <h1 class="sidebar-header">Waterdamp</h1>
    </div>
</template>

<style scoped>
    .sidebar {
        width: 200px;
        height: 100vh;
        position: fixed;
        left: 0;
        top: 0;
        background-color: #f4f4f4;
        padding: 20px;
        background-image: linear-gradient(135deg, #211d67, #8e5e67, #fe879f, #fe1f4c);
        border-radius: 4px;
        z-index: 1000;
    }

    .sidebar-header {
        color: white;
        margin-bottom: 20px;
        position: fixed;
        bottom: 0px;
    }

    .sidebar ul {
        list-style-type: none;
        padding: 0;
    }

    .sidebar ul li {
        margin-bottom: 10px;
    }

    .sidebar ul li a {
        text-decoration: none;
        color: #ffffff; /* Dark blue text color */
        font-weight: bold;
    }

    .sidebar ul li a.router-link-exact-active,
    .sidebar ul li a:hover {
        color: #fe1f4c; /* Text color on hover */
        background-color: transparent; /* Ensure no background color change on hover */
    }

    /* Rounded border */
    hr.rounded {
        border-top: 2px solid #f4f4f4;
        border-radius: 5px;
        margin-bottom: 5px;
    }
</style>
