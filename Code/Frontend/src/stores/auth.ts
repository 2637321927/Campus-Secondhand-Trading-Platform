import {defineStore} from 'pinia'
import {ref, computed} from 'vue'
import {
    getToken,
    setToken,
    deleteToken
} from '../utils/token'
import type {
    CurrentUser,
    LoginRequest
} from '../types/api/auth'
import {
    login as loginApi,
    logout as logoutApi,
    getCurrentUser
} from '../api/modules/auth'

export const useAuthStore=defineStore('auth',()=>{
    const token = ref<string | null>(getToken())
    const currentUser = ref<CurrentUser | null>(null)
    const loading = ref(false)
    const initialized = ref(false)
    const isLoggedIn = computed(() => Boolean(token.value))
    const isAdmin = computed(() => {
        return currentUser.value !== null && currentUser.value.userType === 1
    })

    async function loginAction(data:LoginRequest):Promise<void> {
        loading.value=true
        try{
            const response=await loginApi(data)
            setToken(response.data.token)
            token.value=response.data.token

            const userResponse=await getCurrentUser()
            currentUser.value=userResponse.data
        }
        catch(error){
            clearAuthState()
            throw error
        }
        finally{
            loading.value=false
        }
    }

    async function initializeAuth():Promise<void>{
        if(initialized.value)
            return

        if(!token.value){
            initialized.value=true
            return
        }
        else{
            try{
                const response=await getCurrentUser()
                currentUser.value=response.data
            }
            catch{
                clearAuthState()
            }
            finally{
                initialized.value=true
            }
        }
    }

    function clearAuthState():void{
        deleteToken()
        token.value=null
        currentUser.value=null
    }

    async function logoutAction():Promise<void> {
        loading.value=true
        try{
            await logoutApi()
        }
        finally{
            clearAuthState()
            loading.value=false
        }
    }

    return{
        token,
        currentUser,
        loading,
        initialized,
        isLoggedIn,
        isAdmin,
        loginAction,
        initializeAuth,
        logoutAction
    }
})
