export { Vote, VoteSubject, VoteInput, getVoteInputs, inputVote as tryInputVote, 
        getVotes }

async function getVotes(count = 10, sortOrder = "desc") {
    const response = await fetch(`https://localhost:7000/vote?${count}=10&sortBy=cdt&sortOrder=${sortOrder}`, {
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
    return fetch(`https://localhost:7000/vote?voteId=${voteId}&subjectId=${subjectId}`, {
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
    return fetch("https://localhost:7000/user/vote-inputs", {
        credentials: 'include'
    })
        .then(async response => Promise.resolve({ isSuccess: response.ok, json: await response.json() }))
        .then(response => {
            if (!response.isSuccess) {
                if (response.json.message) {
                    console.log("Failed when fetching vote inputs: ", response.message)
                    return null
                }
            }

            if (!response.json.result) {
                console.log("Succeeded fetching vote inputs but no result object: ", response.message)
                return null
            }

            const result = response.json.result
            return result.map((inp) => new VoteInput(
                inp.id, inp.voteId, inp.voteTitle, inp.subjectId, inp.subjectName, inp.inputTime
            ))
        })
        .catch(error => console.error("Error happened while updating vote inputs", error))
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