export { login, register, isAuthenticated, canManageVote, User }

import { httpFetch, HttpMethod, BaseUrl } from './utils'

const LoginPath = "/login"
const LoginUrl = BaseUrl + LoginPath

const RegisterPath = "/register"
const RegisterUrl = BaseUrl + RegisterPath


const ManageVotesAccessPath = "/votes/can-manage"
const ManageVotesAccessUrl = BaseUrl + ManageVotesAccessPath

async function canManageVote() {
    const result = await httpFetch(ManageVotesAccessUrl)
    return result.isSuccess
}

async function login(email, password) {
    return httpFetch(LoginUrl, HttpMethod.POST, {
        email: email,
        password: password,
        remember: true,
    })
        .then(response => {
            const requestResult = {}
            if (!response.isSuccess) {
                requestResult.errorMessage = response.message
                requestResult.validationErrors = response.validationErrors
            } else {
                const user = response.result
                requestResult.result = new User(
                    user.id,
                    user.email,
                    user.firstName,
                    user.lastName,
                    user.createdTime
                )
            }

            return requestResult
        })
        .catch(error => {
            console.error(`Error happened while logging in user ${email}`, error)
            return { errorMessage: "There was an error while logging in" }
        })
}

async function register(email, password, firstName, lastName) {
    return httpFetch(RegisterUrl, HttpMethod.POST, {
            email: email,
            password: password,
            firstName: firstName,
            lastName: lastName
        })
        .then(response => {
            const requestResult = {}
            if (!response.isSuccess) {
                requestResult.errorMessage = response.message
                requestResult.validationErrors = response.validationErrors
            } else {
                const user = response.result
                requestResult.result = new User(
                    user.id,
                    user.email,
                    user.firstName,
                    user.lastName,
                    user.createdTime
                )
            }

            return requestResult
        })
        .catch(error => {
            console.error(`Error happened while registering user ${email}`, error)
            return { errorMessage: "There was an error while registering" }
        })
}

// Check if a cookie named 'auth' exists
function isAuthenticated() {
    const valid = document.cookie.split(';').some((c) => c.trim().startsWith('auth='))
    return valid
}

class User {
    constructor(id, email, firstName, lastName, createdTime) {
        this._id = id
        this.email = email
        this.firstName
        this.lastName
        this.createdTime
    }
}