import { createRouter, createWebHistory } from 'vue-router'
import VotePage from './VotePage.vue'
import LoginPage from './LoginPage.vue'
import RegisterPage from './RegisterPage.vue'

const routes = [
    { path: '/', name: 'Home', component: VotePage, meta: { requiresAuth: true } },
    { path: '/login', name: 'Login', component: LoginPage },
    { path: '/register', name: 'Register', component: RegisterPage }
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

// Check if a cookie named 'auth' exists
function isAuthenticated() {
    const valid = document.cookie.split(';').some((c) => c.trim().startsWith('auth='))
    return valid
}

router.beforeEach((to, from, next) => {
    if (to.meta.requiresAuth && !isAuthenticated()) {
        next({ name: 'Login' })
    } else {
        next()
    }
})

export default router
