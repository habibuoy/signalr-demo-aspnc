export { Vote, VoteSubject}

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
      || this.maximumCount < this.subjects.reduce((acc, e) => acc + e, 0)
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

async function loadVotes() {
  const response = await fetch("https://localhost:7000/votes?count=10&sortBy=cdt&sortOrder=desc", {
    credentials: "include",
  })

  if (!response.ok) {
    console.log("failed fetching votes")
    return
  }

  const data = await response.json()
  if (!data.result) return

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

  voteList.value.addEventListener("animationend", onVoteListAnimationEnded)

  for (let i = 0; i < mappedVotes.length; i++) {
    votes.value.push(mappedVotes[i])
    await nextTick()
    scrollToEnd()
    await delay(250)
  }
}