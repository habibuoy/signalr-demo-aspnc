import { createRouter, createWebHistory } from 'vue-router'
import VotePage from './VotePage.vue'
import LoginPage from './LoginPage.vue'
import RegisterPage from './RegisterPage.vue'
import { canManageVote, isAuthenticated } from './access'
import ManageVotePage from './ManageVotePage.vue'

const routes = [
    { path: '/', name: 'Home', component: VotePage, meta: { requiresAuth: true } },
    { path: '/login', name: 'Login', component: LoginPage },
    { path: '/register', name: 'Register', component: RegisterPage },
    { path: '/manage-votes', name: 'Manage Vote', component: ManageVotePage, meta: {
        requiresAuth: true,
        requiresManageVotesPrivilege: true,
    }}
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

router.beforeEach(async (to, from, next) => {
    if (to.meta.requiresAuth && !isAuthenticated()) {
        next({ name: 'Login' })
    } 
    else if (to.meta.requiresManageVotesPrivilege && !(await canManageVote())) {
        next({ name: from.name})
    } 
    else {
        next()
    }
})

export default router
