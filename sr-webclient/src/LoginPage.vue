<template>
    <div class="flex flex-col items-center justify-center min-h-screen bg-gray-100">
        <div class="bg-white p-8 rounded shadow-md w-full max-w-sm">
            <h2 class="text-2xl font-bold mb-6 text-center">Login</h2>
            <form @submit.prevent="onLogin">
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

const email = ref('')
const password = ref('')
const router = useRouter()

async function onLogin() {
    console.log("Logging in")
    const result = await login()
    if (result) {
        console.log(`Login successful`)
        router.push('/')
    }
    else
    {
        console.log("Login failed")
    }
}
</script>
