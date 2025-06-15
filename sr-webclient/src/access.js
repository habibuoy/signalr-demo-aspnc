export { login, isAuthenticated, canManageVote }

async function canManageVote() {
    const response = await fetch(`https://localhost:7000/votes/can-manage`, {
        credentials: "include",
    })

    return response.ok
}

async function login(email, password) {
    const loginHeader = new Headers()
    loginHeader.append("Content-Type", "application/json")
    return await fetch("https://localhost:7000/login", {
        method: "POST",
        body: JSON.stringify({
            email: email,
            password: password,
            remember: true,
        }),
        headers: loginHeader,
        credentials: "include"
    })
        .then(async response => Promise.resolve({ isSuccess: response.ok, json: await response.json() }))
        .then(response => {
            if (!response.isSuccess) {
                if (response.json.message) {
                    console.log("Failed logging in: ", response.json.message)
                }

                return null
            }

            if (!response.json.result) {
                console.log("Login successful but no result")
                return null
            }

            return response.json.result
        })
        .catch(error => console.error("Error happeneed while logging in", error))
}

// Check if a cookie named 'auth' exists
function isAuthenticated() {
    const valid = document.cookie.split(';').some((c) => c.trim().startsWith('auth='))
    return valid
}