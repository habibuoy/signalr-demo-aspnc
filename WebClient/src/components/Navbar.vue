<template>
    <nav
        class="bg-white shadow-md py-4 px-8 flex justify-between items-center fixed top-0 left-0 w-full z-40">
        <div class="text-xl font-bold text-blue-600">Vote App</div>
        <div class="navbar-buttons flex justify-between items-center gap-2">
            <button v-show="showManageVote" @click="onClickManageVote"
                class="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600 transition-colors">
                Manage Votes
            </button>
            <button v-show="showLogout" @click="onLogout"
                class="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600 transition-colors">
                Logout
            </button>
        </div>
    </nav>
    <div v-if="showLogoutModal" class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-40 z-50">
        <div class="bg-white p-8 rounded shadow-lg flex flex-col items-center animate-vertical-slide-in">
            <svg class="animate-spin h-8 w-8 text-blue-500 mb-4" xmlns="http://www.w3.org/2000/svg" fill="none"
                viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z"></path>
            </svg>
            <div class="text-lg font-semibold text-gray-700">Logging out...</div>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, onMounted, } from 'vue'
import { useRouter } from 'vue-router'
import { canManageVote } from '../access'

const router = useRouter()
// const showNavbar = computed(() => !['/login', '/register'].includes(router.currentRoute.value.path))

onMounted(() => {
    canManageVote()
        .then(result => {
            showManageVote.value = (result && router.currentRoute.value.path !== "/manage-votes")
        })
})

const showManageVote = ref(false)
const showLogout = ref(true)
const showLogoutModal = ref(false)
const props = defineProps({
    onBeforeLogout: {
        type: Function,
        default: null
    }
})

async function onLogout() {
    showLogout.value = false
    showLogoutModal.value = true
    if (props.onBeforeLogout) {
        // console.log(`awaiting onbeforelogout`)
        await props.onBeforeLogout()
    }
    //   console.log(`Logging out`)
    // Remove auth cookie (set to expired)
    document.cookie = 'auth=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/'
    showLogoutModal.value = false
    showLogout.value = true
    router.push('/login')
}

function onClickManageVote() {
    router.push('/manage-votes')
}

</script>

<style>
body {
    margin-top: 4rem;
    /* Adjust this value based on the navbar height */
}
</style>
