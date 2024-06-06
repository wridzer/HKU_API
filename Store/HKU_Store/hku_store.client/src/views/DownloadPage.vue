<template>
    <div class="download-page">
        <h1>Download Page</h1>
        <div class="download-links">
            <a :href="libraryZipUrl" download>
                <button>Download Library.zip</button>
            </a>
            <a :href="unityExampleZipUrl" download>
                <button>Download UnityExample.zip</button>
            </a>
        </div>
    </div>
</template>

<script>
    export default {
        data() {
            return {
                libraryZipUrl: require('@/assets/Library.zip'),
                unityExampleZipUrl: require('@/assets/UnityExample.zip'),
            };
        },
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
