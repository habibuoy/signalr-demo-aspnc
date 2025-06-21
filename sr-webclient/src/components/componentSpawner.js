import { createApp, h, ref } from "vue";

const appElement = document.querySelector("#app")

export function spawnComponent(component, props = {}, options = {}, attachElement = null) {
    const container = document.createElement("div")
    
    const element = attachElement ?? appElement
    element.appendChild(container)

    if (options.zIndex !== null) {
        container.style.zIndex = options.zIndex
        container.style.position = 'fixed'
        container.style.top = '0'
        container.style.left = '0'
    }

    const requiredProps = Object.entries(component.props).filter(([key, value]) => value.required).map(([key, value]) => key)
    const missingRequiredProps = requiredProps.reduce((acc, e, i) => {
        if (!(e in props))
        {
            acc += e + ", "
        }
        return acc
    }, "")

    if (missingRequiredProps.length > 0) {
        throw new Error(`You missed required prop ${missingRequiredProps.substring(0, missingRequiredProps.length - 2)} while spawning component ${component.__name}`)
        // console.warn(`You missed required prop ${missingRequiredProps.substring(0, missingRequiredProps.length - 2)} while spawning component ${component.__name}`)
    }
    
    const componentRef = ref(null)
    const app = createApp({
        render() {
            return h(component, {
                ...props,
                onClose: () => {
                    destroy()
                },
                ref: componentRef
            })
        }
    })

    let onDestroySubscribers = new Set()
    
    const destroy = () => {
        app.unmount()
        element.removeChild(container)

        onDestroySubscribers.forEach(sub => {
            if (typeof(sub) === "function") {
                sub()
            }
        })

        onDestroySubscribers.clear()
        onDestroySubscribers = null
    }

    app.mount(container)

    return {
        instance: componentRef.value,
        destroy,
        onDestroy: {
            subscribe: (fn) => {
                onDestroySubscribers.add(fn)
            },
            unsubscribe: (fn) => {
                onDestroySubscribers.delete(fn)
            }
        }
    }
}