<template>
    <div class="download-page">
        <h1>Download Page</h1>
        <div class="download-links">
            <a href="/Library.zip" download>
                <button>Download Library.zip</button>
            </a>
            <a href="/UnityExample.zip" download>
                <button>Download UnityExample.zip</button>
            </a>
        </div>
    </div>
</template>

<script>
    export default {
        name: 'DownloadPage'
    };
</script>

<style scoped>
    .download-page {
        text-align: center;
        padding: 20px;
    }

    .download-links {
        margin-top: 20px;
    }

        .download-links a {
            margin: 10px;
        }

    button {
        padding: 10px 20px;
        font-size: 16px;
        cursor: pointer;
    }
</style>
