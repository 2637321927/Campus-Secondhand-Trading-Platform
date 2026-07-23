export function formatDate(data:string):string{
    const date=new Date(data)

    if(Number.isNaN(date.getTime())){
       return ''
    }
    else{
        return date.toLocaleDateString('zh-CN', {
            month: '2-digit',
            day: '2-digit'
        })
    }
}