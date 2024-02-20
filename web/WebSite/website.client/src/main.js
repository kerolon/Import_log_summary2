import './assets/main.css'

import { createApp} from 'vue'
import App from './App.vue'
import vue3GoogleLogin from 'vue3-google-login'

const app = createApp(App, {
    env: import.meta.env
})
app.use(vue3GoogleLogin, {
    clientId: import.meta.env.VITE_ENV_CLIENT_ID
})
app.mount('#app')
app.config.errorHandler = (err, instance, info) => {
    //クライアントでもエラーログが見れるように敢えてコンソールに出力
    console.log(err);
    console.log(instance);
    console.log(info);
}