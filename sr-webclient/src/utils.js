export { delay, calculatePercentage, httpFetch, HttpMethod }

async function delay(durationInMs) {
    return new Promise(resolve => setTimeout(resolve, durationInMs))
}

function calculatePercentage(count, total, decimalPlace = 2) {
    if (typeof count !== 'number'
        || typeof total !== 'number'
        || total === 0) {
        return 0
    }

    return (count / total * 100).toFixed(decimalPlace)
}

const httpHeaders = new Headers({ "Content-Type": "application/json" })

const HttpMethod =  {
    GET: "GET",
    POST: "POST",
    PUT: "PUT",
    DELETE: "DELETE"
}

async function httpFetch(url, method = HttpMethod.GET, bodyObject = { }, headers = { }, credentials = "include") {
    const jsonBody = bodyObject && Object.keys(bodyObject).length > 0 
        ? JSON.stringify(bodyObject, null, "\t") : undefined

    return fetch(url, {
        method: method,
        body: jsonBody,
        credentials: credentials,
        headers: headers && Object.keys(headers).length > 0 
            ? new Headers({ ...headers })
            : httpHeaders
    })
        .then(async response => {
            const responseJson = await response.json()
            const message = response.status === 500 ? "Server error" : responseJson.message
            const result = responseJson.result
            const validationErrors = result ? result.validationErrors : undefined

            if (!response.ok) {
                console.error(`Failed fetching url ${url} (status code ${response.status}) : ${message}`, validationErrors)
            }

            return Promise.resolve({ 
                isSuccess: response.ok,
                statusCode: response.status,
                message: message,
                result: result,
                validationErrors: validationErrors
            })
        })
        .catch(error => {
            console.error(`Error happened while fetching url ${url}`, error)
        })
}
