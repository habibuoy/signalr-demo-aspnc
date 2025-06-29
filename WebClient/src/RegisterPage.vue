<template>
    <div class="flex flex-col items-center justify-center min-h-screen bg-gray-100">
        <div class="bg-white p-8 rounded shadow-md w-full max-w-sm">
            <h2 class="text-2xl font-bold mb-6 text-center">Register</h2>
            <form id="register-form" @submit.prevent="onRegisterClicked">
                <div class="mb-4">
                    <label class="block mb-1 text-gray-700">Email</label>
                    <input v-model="email" type="email" class="w-full px-3 py-2 border rounded" required
                        name="email" @input="onEmailInput" />
                </div>
                <div class="mb-4">
                    <label class="block mb-1 text-gray-700">Password</label>
                    <input v-model="password" type="password" class="w-full px-3 py-2 border rounded" required
                        name="password" @input="onPasswordInput" />
                </div>
                <div class="mb-4">
                    <label class="block mb-1 text-gray-700">First Name</label>
                    <input v-model="firstName" type="text" class="w-full px-3 py-2 border rounded" required
                        name="firstName" @input="onFirstNameInput" />
                </div>
                <div class="mb-6">
                    <label class="block mb-1 text-gray-700">Last Name</label>
                    <input v-model="lastName" type="text" class="w-full px-3 py-2 border rounded" required
                        name="lastName" @input="onLastNameInput" />
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
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { register } from './access'
import { spawnLoading } from './components/loading'
import { spawnResultPopup } from './components/resultPopup'

const email = ref('')
const password = ref('')
const firstName = ref('')
const lastName = ref('')
const router = useRouter()

const PasswordMinimumLength = 8
const PasswordOneUppercaseRequirementRegexString = "(?=.*?[A-Z])";
const PasswordOneLowercaseRequirementRegexString = "(?=.*?[a-z])";
const PasswordOneNumberRequirementRegexString = "(?=.*?[0-9])";
const PasswordOneSpecialCharacterRequirementRegexString = "(?=.*?[#?!@$%^&*-])";

const passwordOneUppercaseRegex = new RegExp(PasswordOneUppercaseRequirementRegexString)
const passwordOneLowercaseRegex = new RegExp(PasswordOneLowercaseRequirementRegexString)
const passwordOneNumberRegex = new RegExp(PasswordOneNumberRequirementRegexString)
const passwordOneSpecialCharacterRegex = new RegExp(PasswordOneSpecialCharacterRequirementRegexString)

const formInputs = {}

onMounted(() => {
    const form = document.getElementById('register-form')
    const inputs = form.getElementsByTagName('input')
    for (const input of inputs) {
        formInputs[input.name] = input
    }
})

function invalidateForm(inputs = {}) {
    for (const input in inputs) {
        const inputElement = formInputs[input]
        inputElement.setCustomValidity(inputs[input].join('\n'))
        inputElement.reportValidity()
    }
}

function onEmailInput(event) {
    const inputElement = event.target
    const inputValue = inputElement.value

    inputElement.setCustomValidity("")
}

function onPasswordInput(event) {
    const inputElement = event.target
    const inputValue = inputElement.value

    inputElement.setCustomValidity("")
    if (inputValue === undefined) {
        return
    }

    let validityText = ""
    if (inputValue.length < PasswordMinimumLength) {
        validityText += `Password length should at least ${PasswordMinimumLength} characters\n`
    }

    if (!passwordOneUppercaseRegex.test(inputValue)) {
        validityText += "Password should contain at least one uppercase character\n"
    }

    if (!passwordOneLowercaseRegex.test(inputValue)) {
        validityText += "Password should contain at least one lowercase character\n"
    }

    if (!passwordOneNumberRegex.test(inputValue)) {
        validityText += "Password should contain at least one number (0 - 9)\n"
    }

    if (!passwordOneSpecialCharacterRegex.test(inputValue)) {
        validityText += "Password should contain at least one special character (#?!@$%^&*-)"
    }

    inputElement.setCustomValidity(validityText)
}

function onFirstNameInput(event) {
    const inputElement = event.target
    const inputValue = inputElement.value

    inputElement.setCustomValidity("")
}

function onLastNameInput(event) {
    const inputElement = event.target
    const inputValue = inputElement.value

    inputElement.setCustomValidity("")
}

async function onRegisterClicked() {
    const loading = spawnLoading({loadingText: "Registering..."}, 20)

    try {
        const result = await register(email.value, password.value, firstName.value, lastName.value)
        const success = result && !result.errorMessage

        let feedbackText = "Successfully registered"
        if (!success) {
            feedbackText = `Failed: ${result.errorMessage}`
            if (result.validationErrors) {
                invalidateForm(result.validationErrors)
            }
        }

        const popup = spawnResultPopup({ feedbackText, success })
        popup.onDestroy.subscribe(() => { if (success) router.push('/login') })
    } catch (error) {
        console.error(`Error happened while registering user ${email}`, error)
        spawnResultPopup({ feedbackText: "Error registering", success: false })
    } finally {
        loading.destroy()
    }
}
</script>
