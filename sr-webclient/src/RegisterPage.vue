<template>
    <div class="flex flex-col items-center justify-center min-h-screen bg-gray-100">
        <div class="bg-white p-8 rounded shadow-md w-full max-w-sm">
            <h2 class="text-2xl font-bold mb-6 text-center">Register</h2>
            <form @submit.prevent="onRegister">
                <div class="mb-4">
                    <label class="block mb-1 text-gray-700">Email</label>
                    <input v-model="email" type="email" class="w-full px-3 py-2 border rounded" required />
                </div>
                <div class="mb-4">
                    <label class="block mb-1 text-gray-700">Password</label>
                    <input v-model="password" type="password" class="w-full px-3 py-2 border rounded" required />
                </div>
                <div class="mb-4">
                    <label class="block mb-1 text-gray-700">First Name</label>
                    <input v-model="firstName" type="text" class="w-full px-3 py-2 border rounded" required />
                </div>
                <div class="mb-6">
                    <label class="block mb-1 text-gray-700">Last Name</label>
                    <input v-model="lastName" type="text" class="w-full px-3 py-2 border rounded" required />
                </div>
                <button type="submit"
                    class="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600">Register</button>
            </form>
            <div class="mt-4 text-center">
                <router-link to="/login" class="text-blue-500 hover:underline">Already have an account?
                    Login</router-link>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const email = ref('')
const password = ref('')
const firstName = ref('')
const lastName = ref('')
const router = useRouter()

async function register(email, password, firstName, lastName) {
    const loginHeader = new Headers()
    loginHeader.append("Content-Type", "application/json")
    const result = await fetch("https://localhost:7000/register", {
        method: "POST",
        body: JSON.stringify({
            email: email,
            password: password,
            firstName: firstName,
            lastName: lastName
        }),
        headers: loginHeader
    })
        .then(response => {
            if (response.ok) {
                return true
            }

            return false
        })
        .catch(error => console.error("Error while registering", error))

    return result
}

async function onRegister() {
    // Use the register function
    const result = await register(email.value, password.value, firstName.value, lastName.value)
    if (result) {
        router.push('/login')
    }
    else {
        console.log("Failed when registering")
    }
}
</script>
