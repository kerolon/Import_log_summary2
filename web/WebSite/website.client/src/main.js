import './assets/main.css'

import { createApp} from 'vue'
import App from './App.vue'
import vue3GoogleLogin from 'vue3-google-login'

const app = createApp(App);
app.use(vue3GoogleLogin, {
    clientId: '735794523893-coj9o5ckteib35ukdn1la9qhfrv8g5c5.apps.googleusercontent.com'
})
app.mount('#app')
