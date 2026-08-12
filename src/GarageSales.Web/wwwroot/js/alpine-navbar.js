function navbarLogic(){
    return{
        async logout() {
            try {
                const response = await fetch('https://localhost:7285/api/auth/logout', 
                { 
                    method: 'POST',
                    credentials: 'include' 
                });
                if (response.ok) {
                    window.location.href = '/'; // Redirect to home page after sign-out
                }
            } catch (err) {
                console.error('Logout failed', err);
            }
        }  
    }
}