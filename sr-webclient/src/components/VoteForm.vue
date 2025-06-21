<template e>
    <div class="relative" aria-labelledby="modal-title" role="dialog"
        aria-modal="true">
        <div class="fixed inset-0 bg-gray-500/75 transition-opacity" aria-hidden="true"></div>
        <div class="fixed inset-0 w-screen overflow-y-auto text-center">
            <div class="flex min-h-full justify-center p-4 text-center items-center sm:p-0">
                <div class="bg-white p-8 rounded shadow-md w-full max-w-sm">
                    <h2 class="text-2xl font-bold mb-6 text-center">{{ props.formTitle }}</h2>
                    <form id="vote-form" @submit.prevent="onPositiveClicked">
                        <div class="mb-4">
                            <label class="block mb-1 text-gray-700">Title</label>
                            <input v-model="voteTitle" type="text" class="w-full px-3 py-2 border rounded" required />
                        </div>
                        <div class="mb-4">
                            <label class="block mb-1 text-gray-700">Subjects (separate with comma)</label>
                            <input v-model="voteSubjects" type="text" class="w-full px-3 py-2 border rounded"
                                required @input="validateSubjectsInput"/>
                        </div>
                        <div class="mb-6">
                            <label class="block mb-1 text-gray-700">Duration (in seconds, 0 = no duration)</label>
                            <input placeholder="0" v-model="voteDuration" type="number"
                                class="w-full px-3 py-2 border rounded" />
                        </div>
                        <div class="mb-6">
                            <label class="block mb-1 text-gray-700">Maximum count (0 = no max)</label>
                            <input placeholder="0" v-model="voteMaxCount" type="number"
                                class="w-full px-3 py-2 border rounded" @input="validateMaximumCountInput" @focus="validateMaximumCountInput" />
                        </div>
                        <div class="bg-white-50 py-3 flex justify-center gap-2">
                            <button type="button" @click="onCloseClicked" class="mt-3 inline-flex w-full justify-center rounded-md 
                                bg-white px-3 py-2 text-sm font-semibold 
                                text-gray-900 shadow-xs ring-1 ring-gray-300 ring-inset 
                                hover:bg-gray-50 sm:mt-0 sm:w-auto">
                                Cancel
                            </button>
                            <button type="submit" class="inline-flex w-full justify-center rounded-md 
                                bg-blue-600 px-3 py-2 text-sm font-semibold 
                                text-white shadow-xs 
                                hover:bg-blue-500 sm:ml-3 sm:w-auto">
                                {{ props.positiveText }}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';

const props = defineProps({
    formTitle: {
        type: String,
        default: "Form Title",
        required: true
    },
    id: {
        type: String,
        default: null
    },
    title: {
        type: String,
        default: "Vote Title",
    },
    subjects: {
        type: Array,
        default: []
    },
    duration: {
        type: Number,
        default: 0
    },
    maxCount: {
        type: Number,
        default: 0
    },
    closeOnPositive: {
        type: Boolean,
        default: true
    },
    positiveText: {
        type: String,
        default: 'OK'
    },
    negativeText: {
        type: String,
        default: 'Close'
    },
    readonly: {
        type: Boolean,
        default: false
    }
})

const emit = defineEmits(["positive", "close"])

const validMinSubjectCount = 2
const voteTitle = ref(props.title)
const voteSubjects = ref(props.subjects.reduce((acc, elm) => {
    acc.push(elm.name)
    return acc
}, []))
const voteDuration = ref(props.duration)
const voteMaxCount = ref(props.maxCount)

const voteSubjectIds = []

onMounted(() => {
    if (props.subjects.length > 0) {
        for (const subj of props.subjects) {
            voteSubjectIds.push(subj.id)
            subjects.push(subj.name)
        }
    }

    if (props.readonly) {
        const form = document.getElementById('vote-form')
        const inputs = form.getElementsByTagName('input')
        for (let i = 0; i < inputs.length; i++) {
            inputs.item(i).disabled = true
        }
    }
})

let currentSubjectCount = 0
let subjects = []

function isSubjectCountValid() { 
    return +currentSubjectCount >= validMinSubjectCount 
}

function validateSubjectsInput(event) {
    const inputElement = event.target
    const inputValue = inputElement.value
    subjects = inputValue.split(",").reduce((acc, e) => {
        e = e.trim()
        if (e.length > 0) {
            acc.push(e)
        }
        return acc
    }, [])
    currentSubjectCount = subjects.length

    if (!isSubjectCountValid()) {
        inputElement.setCustomValidity("Please provide at least 2 subjects")
    }
    else if (voteSubjectIds.length > 0
        && currentSubjectCount != voteSubjectIds.length) {
        inputElement.setCustomValidity(`Please provide the same amount of subjects as before (${voteSubjectIds.length})`)
    }
    else
    {
        inputElement.setCustomValidity("")
    }
}

function validateMaximumCountInput(event) {
    const inputElement = event.target
    const inputValue = inputElement.value

    if (inputValue === 0) {
        inputElement.setCustomValidity("")
        return
    }

    if (!isSubjectCountValid()) {
        inputElement.setCustomValidity("Please fill valid subjects first")
        return
    }

    if (inputValue % currentSubjectCount !== 0) {
        inputElement.setCustomValidity("Please provide a valid maximum count (remainder of max count divided by subject count should be 0)")
    }
    else
    {
        inputElement.setCustomValidity("")
    }
}

function onPositiveClicked() {
    emit("positive", { 
        voteId: props.id,
        voteTitle: voteTitle.value, 
        voteSubjects: subjects.reduce((acc, elm, index) => {
            acc.push({ id: voteSubjectIds.length > 0 ? voteSubjectIds[index] : null, name: elm })
            return acc
        }, []), 
        voteDuration: voteDuration.value, 
        voteMaxCount: voteMaxCount.value 
    })
    
    if (props.closeOnPositive === true) {
        emit("close")
    }
}

function onCloseClicked() {
    emit("close")
}

</script>