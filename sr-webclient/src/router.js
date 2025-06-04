import { createRouter, createWebHistory } from 'vue-router'
import VotePage from './VotePage.vue'
import LoginPage from './LoginPage.vue'
import RegisterPage from './RegisterPage.vue'
import { isAuthenticated } from './access'

const routes = [
    { path: '/', name: 'Home', component: VotePage, meta: { requiresAuth: true } },
    { path: '/login', name: 'Login', component: LoginPage },
    { path: '/register', name: 'Register', component: RegisterPage }
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

router.beforeEach((to, from, next) => {
    if (to.meta.requiresAuth && !isAuthenticated()) {
        next({ name: 'Login' })
    } else {
        next()
    }
})

export default router
