import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import router from './router' // Importeer je router configuratie

createApp(App).use(router).mount('#app')
