<template>
    <Navbar></Navbar>
    <div :class="['mv-container', showVotes ? '' : 'centered']">
        <div :class="['mv-panel', showVotes ? 'expanded' : 'collapsed']">
            <h2 class="text-2xl font-bold mb-4 text-center">Manage Votes</h2>
            <div class="flex flex-col gap-2">
                <button @click="onCreateVoteClicked" type="button"
                    class="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600">
                    Create Vote
                </button>
                <button @click="onShowVotesClicked" type="button"
                    class="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600">
                    {{ showVotes ? 'Hide votes' : 'Show all votes' }}
                </button>
            </div>

            <div v-if="showVotes" class="mv-list">
                <div v-if="isLoading" class="flex items-center justify-center py-8">
                    <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mr-3"></div>
                    <span class="text-gray-600">Loading votes...</span>
                </div>
                <div v-else-if="votes.length === 0" class="text-center py-8 text-gray-600">
                    No votes available
                </div>
                <div v-else-if="votes === null || !votes.length" class="text-center py-8 text-gray-600">
                    Error getting vote list
                </div>
                <div v-else v-for="vote in votes" :key="vote.id"
                    :class="['mv-item', { selected: selectedVoteId === vote.id }]" @click="onVoteItemClicked(vote.id)">
                    <div class="flex justify-between items-start">
                        <div>
                            <h3 class="text-lg font-semibold">{{ vote.title }}</h3>
                            <button @click.stop="onVoteSubjectsClicked(vote.id)" class="mv-subject-count">
                                {{ vote.subjects.length }} subjects
                            </button>
                        </div>
                        <div class="text-right">
                            <div class="text-sm text-gray-600">
                                Total votes: {{ vote.totalCount }}
                            </div>
                            <div v-if="vote.maximumCount" class="text-sm text-gray-600">
                                Maximum votes: {{ vote.maximumCount }}
                            </div>
                            <div v-if="vote.expiredTime" class="text-sm text-gray-600">
                                Close at: {{ formatDateTime(vote.expiredTime) }}
                            </div>
                        </div>
                    </div>

                    <div :class="['mv-subjects', { expanded: expandedSubjectsId === vote.id }]">
                        <div v-for="subject in vote.subjects" :key="subject.id" class="mv-subject">
                            <span>{{ subject.name }}</span>
                            <span class="text-gray-600">{{ subject.voteCount }} votes
                                ({{ calculatePercentage(subject.voteCount, vote.totalCount) }}%)</span>
                        </div>
                    </div>

                    <div :class="['mv-actions', { visible: selectedVoteId === vote.id }]">
                        <div class="flex gap-2">
                            <button @click.stop="onEditClicked(vote)"
                                class="bg-blue-500 text-white px-4 py-1 rounded hover:bg-blue-600">
                                Edit
                            </button>
                            <button @click.stop="onDeleteClicked(vote)"
                                class="bg-red-500 text-white px-4 py-1 rounded hover:bg-red-600">
                                Delete
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref } from 'vue'
import { spawnLoading } from './components/loading'
import Navbar from './components/Navbar.vue'
import { delay, calculatePercentage, formatDateTime } from './utils'
import { spawnResultPopup } from './components/resultPopup'
import { spawnComponent } from './components/componentSpawner'
import VoteForm from './components/VoteForm.vue'
import { createNewVote, getVotes, updateVote, deleteVote } from './vote'

let voteForm = null
const showVotes = ref(false)
const selectedVoteId = ref(null)
const expandedSubjectsId = ref(null)
const votes = ref([] | null)
const isLoading = ref(false)

function onVoteItemClicked(id) {
    selectedVoteId.value = selectedVoteId.value === id ? null : id
}

function onVoteSubjectsClicked(id) {
    expandedSubjectsId.value = expandedSubjectsId.value === id ? null : id
}

async function fetchVotes() {
    isLoading.value = true
    try {
        const result = await getVotes(25)
        if (result.errorMessage) {
            votes.value = null
            return
        }

        votes.value = result.result
    } catch (error) {
        console.error('Failed to fetch votes:', error)
        spawnResultPopup({
            feedbackText: 'Failed to load votes',
            success: false
        })
    } finally {
        isLoading.value = false
    }
}

function onCreateVoteClicked() {
    voteForm = spawnComponent(VoteForm, {
        formTitle: "Create a new Vote", 
        closeOnPositive: false, 
        onPositive: proceedCreateVote
    }, { zIndex: "20" })
    voteForm.onDestroy.subscribe(() => voteForm = null)
}

async function proceedCreateVote(data) {
    if (!data) {
        console.log("no data return from vote create form")
        return
    }

    const loading = spawnLoading({ loadingText: "Creating new vote..." }, "20")
    const subjects = data.voteSubjects.reduce((acc, elm) => {
        acc.push(elm.name)
        return acc
    }, [])

    try {
        const createResult = await createNewVote(data.voteTitle, subjects, data.voteDuration, data.voteMaxCount)
        const success = createResult && !createResult.errorMessage
        let feedbackText = `Successfully created new vote ${data.voteTitle}`

        if (!success) {
            feedbackText = `Failed: ${createResult.errorMessage}`
        }

        spawnResultPopup({
            feedbackText,
            success
        })

        if (!success) {
            if (createResult.validationErrors) {
                voteForm.instance.invalidateForm(createResult.validationErrors)
            }
            return
        }

        if (voteForm && voteForm.destroy) {
            voteForm.destroy()
        }

        if (showVotes.value) {
            await fetchVotes()
        }
    } catch (error) {
        spawnResultPopup({ feedbackText: "Error creating vote", success: false })
        console.error("Error happened while creating vote", error)
    } finally {
        loading.destroy()
    }
}

async function onShowVotesClicked() {
    showVotes.value = !showVotes.value
    if (showVotes.value) {
        await fetchVotes()
    } else {
        selectedVoteId.value = null
        expandedSubjectsId.value = null
    }
}

function onEditClicked(vote) {
    voteForm = spawnComponent(VoteForm, {
        id: vote.id,
        formTitle: `Update vote ${vote.title}`,
        title: vote.title,
        subjects: vote.subjects.reduce((acc, elm) => {
            acc.push({ id: elm.id, name: elm.name })
            return acc
        }, []),
        duration: vote.duration ? vote.duration : 0,
        maxCount: vote.maximumCount ? vote.maximumCount : 0,
        closeOnPositive: false,
        positiveText: "Confirm Edit",
        onPositive: proceedEditVote
    })
    voteForm.onDestroy.subscribe(() => voteForm = null)
}

async function proceedEditVote(data) {
    if (!data) {
        console.log("no data return from vote update form")
        return
    }

    const loading = spawnLoading({ loadingText: "Updating vote..." }, "20");

    try {
        const result = await updateVote(data.voteId, data.voteTitle,
            data.voteSubjects, data.voteDuration, data.voteMaxCount)

        let feedbackText = "Successfully updated vote"
        const success = result && !result.errorMessage

        if (!success) {
            feedbackText = `Failed: ${result.errorMessage}`
        }

        spawnResultPopup({ feedbackText, success }, "21")

        if (!success) {
            if (result.validationErrors) {
                voteForm.instance.invalidateForm(result.validationErrors)
            }
            return
        }

        if (voteForm && voteForm.destroy) {
            voteForm.destroy()
        }

        await fetchVotes()
    } catch (error) {
        console.error(`Error happened while deleting vote ${data.voteId}`, error)
        spawnResultPopup({ feedbackText: "Error updating vote", success: false })
    } finally {
        loading.destroy()
    }
}

function onDeleteClicked(vote) {
    voteForm = spawnComponent(VoteForm, {
        id: vote.id,
        formTitle: `Are you sure want to DELETE this vote?`,
        title: vote.title,
        subjects: vote.subjects.reduce((acc, elm) => {
            acc.push({ id: elm.id, name: elm.name })
            return acc
        }, []),
        duration: vote.duration ? vote.duration : 0,
        maxCount: vote.maximumCount ? vote.maximumCount : 0,
        closeOnPositive: false,
        positiveText: "Confirm",
        onPositive: proceedDeleteVote,
        readonly: true
    })
    voteForm.onDestroy.subscribe(() => voteForm = null)
}

async function proceedDeleteVote(data) {
    if (!data) {
        console.log("no data return from vote update form")
        return
    }

    const loading = spawnLoading({ loadingText: "Deleting vote..." }, "20");

    try {
        const result = await deleteVote(data.voteId)

        const success = result && !result.errorMessage
        let feedbackText = "Successfully deleted vote"

        if (!success) {
            feedbackText = `Failed: ${result.errorMessage}`
        }

        spawnResultPopup({ feedbackText, success }, "21")
        
        if (success) {
            if (voteForm.destroy) {
                voteForm.destroy()
            }

            await fetchVotes()
        }
    } catch (error) {
        console.error(`Error happened while deleting vote ${data.voteId}`, error)
        spawnResultPopup({ feedbackText: "Error deleting vote", success: false }, "21")
    } finally {
        loading.destroy()
    }
}
</script>