export { Vote, VoteSubject, VoteInput, getVoteInputs, inputVote as tryInputVote, 
        getVotes, createNewVote, updateVote, deleteVote }

async function getVotes(count = 10, sortOrder = "desc") {
    const response = await fetch(`https://localhost:7000/votes?count=${count}&sortBy=cdt&sortOrder=${sortOrder}`, {
        credentials: "include",
    })

    if (!response.ok) {
        console.log("failed fetching votes")
        return null
    }

    const data = await response.json()
    if (!data.result) return null

    const mappedVotes = data.result.map(vote => {
        const v = new Vote(
            vote.id,
            vote.title,
            vote.subjects.map(subject => new VoteSubject(
                subject.id,
                subject.name,
                subject.voteCount
            )),
            vote.maximumCount,
            vote.expiredTime
        )
        v.totalCount = vote.subjects.reduce((sum, subject) => sum + subject.voteCount, 0)
        return v
    })

    return mappedVotes
}

async function inputVote(voteId, subjectId) {
    return fetch(`https://localhost:7000/votes/inputs?voteId=${voteId}&subjectId=${subjectId}`, {
        method: "POST",
        credentials: "include"
    })
        .then(async response => {
            const jsonBody = await response.json()
            if (!response.ok) {
                if (jsonBody.message) {
                    console.log(`Failed when inputting vote (Code ${result.status}). Message: ${jsonBody.message}`)
                    return false
                }
            }

            return true
        })
        .catch(error => console.error("Error happened while inputting vote", error))
}

async function getVoteInputs() {
    return fetch("https://localhost:7000/users/vote-inputs", {
        credentials: 'include'
    })
        .then(async response => Promise.resolve({ isSuccess: response.ok, json: await response.json() }))
        .then(response => {
            if (!response.isSuccess) {
                if (response.json.message) {
                    console.log("Failed when fetching vote inputs: ", response.json.message)
                    return null
                }
            }

            if (!response.json.result) {
                console.log("Succeeded fetching vote inputs but no result object: ", response.json.message)
                return null
            }

            const result = response.json.result
            return result.map((inp) => new VoteInput(
                inp.id, inp.voteId, inp.voteTitle, inp.subjectId, inp.subjectName, inp.inputTime
            ))
        })
        .catch(error => console.error("Error happened while updating vote inputs", error))
}

async function createNewVote(title, subjects, duration = 0, maxCount = 0) {
    const jsonBody = JSON.stringify(new VoteCreate(title, subjects, duration, maxCount), null, "\t")
    const reqHeader = new Headers()
    reqHeader.append("Content-Type", "application/json")
    return fetch("https://localhost:7000/votes/", {
        method: "POST",
        credentials: "include",
        headers: reqHeader,
        body: jsonBody
    })
        .then(async response => Promise.resolve({ isSuccess: response.ok, json: await response.json() }))
        .then(response => {
            if (!response.isSuccess) {
                if (response.json.message) {
                    console.log("Failed when creating new vote: ", response.json.message)
                    return null
                }
            }

            const result = response.json.result
            if (!result) {
                console.log("Succeeded fetching vote inputs but no result object: ", response.json.message)
                return null
            }

            return new Vote(result.id, result.title, result.subjects, result.maximumCount, result.expiredTime)
        })
        .catch(error => console.error("Error happened while creating new vote", error))
}

async function updateVote(id, title, subjects, duration, maxCount) {
    const updateRequest = new VoteUpdateRequest(title, subjects, duration, maxCount)
    const jsonBody = JSON.stringify(updateRequest, null, "\t")
    const reqHeader = new Headers()
    reqHeader.append("Content-Type", "application/json")
    return fetch(`https://localhost:7000/votes/${id}`, {
        method: "PUT",
        credentials: "include",
        headers: reqHeader,
        body: jsonBody
    })
        .then(async response => Promise.resolve({ isSuccess: response.ok, json: await response.json() }))
        .then(response => {
            if (!response.isSuccess) {
                if (response.json.message) {
                    if (response.json.result.validationErrors) {
                        console.log(`Failed when updating vote ${id}: ` +
                            `${response.json.message}`, response.json.result.validationErrors)
                    } else {
                        console.log(`Failed when updating vote ${id}: `, response.json.message)
                    }
                    return null
                }
            }

            return updateRequest
        })
        .catch(error => console.error(`Error happened while updating vote ${id}.`, error))
}

async function deleteVote(id) {
    const reqHeader = new Headers()
    reqHeader.append("Content-Type", "application/json")
    return fetch(`https://localhost:7000/votes/${id}`, {
        method: "DELETE",
        credentials: "include",
        headers: reqHeader,
    })
        .then(async response => Promise.resolve({ isSuccess: response.ok, json: await response.json() }))
        .then(response => {
            if (!response.isSuccess) {
                if (response.json.message) {
                    console.log(`Failed when deleting vote ${id}: `, response.json.message)
                    return null
                }
            }

            return id
        })
        .catch(error => console.error(`Error happened while updating vote ${id}.`, error))
}

class Vote {
    totalCount = 0;

    constructor(id, title, subjects, maxVotes, endTime) {
        this.id = id
        this.title = title
        this.subjects = subjects
        this.maximumCount = maxVotes
        this.expiredTime = endTime
    }

    canVote() {
        return (!this.expiredTime
            || Date.parse(this.expiredTime) > Date.now()
        )
            && (!this.maximumCount
                || this.totalCount < this.maximumCount
            )
    }
}

class VoteSubject {
    constructor(id, name, voteCount) {
        this.id = id
        this.name = name
        this.voteCount = voteCount
    }
}

class VoteInput {
    constructor(id, voteId, voteTitle, subjectId, subjectName, inputTime) {
        this.id = id
        this.voteId = voteId
        this.voteTitle = voteTitle
        this.subjectId = subjectId
        this.subjectName = subjectName
        this.inputTime = inputTime
    }
}

class VoteCreate {
    constructor(title, subjects, duration = 0, maxCount = 0) {
        this.title = title,
        this.subjects = subjects,
        this.duration = duration > 0 ? duration : null,
        this.maximumCount = maxCount > 0 ? maxCount : null
    }
}

class VoteUpdateRequest {
    constructor(title, subjects, duration, maxCount) {
        this.title = title,
        this.subjects = subjects,
        this.duration = duration,
        this.maximumCount = maxCount
    }
}