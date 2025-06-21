<template>
    <div class="flex flex-col items-center justify-center min-h-screen bg-gray-100">
        <div class="bg-white p-8 rounded shadow-md w-full max-w-sm">
            <h2 class="text-2xl font-bold mb-6 text-center">Login</h2>
            <form @submit.prevent="onLoginClicked">
                <div class="mb-4">
                    <label class="block mb-1 text-gray-700">Email</label>
                    <input v-model="email" type="email" class="w-full px-3 py-2 border rounded" required />
                </div>
                <div class="mb-6">
                    <label class="block mb-1 text-gray-700">Password</label>
                    <input v-model="password" type="password" class="w-full px-3 py-2 border rounded" required />
                </div>
                <button type="submit"
                    class="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600">Login</button>
            </form>
            <div class="mt-4 text-center">
                <router-link to="/register" class="text-blue-500 hover:underline">Don't have an account?
                    Register</router-link>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { login } from './access'
import { spawnLoading } from './components/loading'
import { spawnResultPopup } from './components/resultPopup'

const email = ref('')
const password = ref('')
const router = useRouter()

async function onLoginClicked() {
    const loading = spawnLoading({loadingText: "Logging in..."}, 20)

    try {
        const result = await login(email.value, password.value)
        const success = result && !result.errorMessage
    
        let feedbackText = "Successfully logged in"

        if (!success) {
            feedbackText = `Failed: ${result.errorMessage}`
        }

        const popup = spawnResultPopup({ feedbackText, success })
        popup.onDestroy.subscribe(() => router.push('/'))
    } catch (error) {
        console.error(`Error happened while logging in ${email}`, error)
        spawnResultPopup({ feedbackText: "Error logging in", success: false })
    } finally {
        loading.destroy()
    }
}
</script>
