import axios from 'axios'
import {getToken,deleteToken} from '../utils/token'

const request=axios.create({
    baseURL:import.meta.env.VITE_API_BASE_URL,
    timeout:10000
})

request.interceptors.request.use(config=>{
    const token=getToken()

    if(token){
        config.headers.Authorization = `Bearer ${token}`
    }

    return config
})

request.interceptors.response.use(
    (response)=>{
        return response
    },
    (error)=>{
        if(!error.response){
            return Promise.reject(error)
        }

        if(error.response.status ===401){
            deleteToken()
        }
        return  Promise.reject(error)
    }
)

export default request