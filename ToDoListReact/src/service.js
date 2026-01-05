import axios from 'axios';

axios.defaults.baseURL = "http://localhost:5125/api";


axios.interceptors.request.use(config => {
    const token = localStorage.getItem("token");
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export default {
login: async (username, password) => {
    const result = await axios.post('/login', { username, password }); 
    
    if (result.data && result.data.token) {
        localStorage.setItem("token", result.data.token);
    }
    
    return result.data;
},
    register: async (username, password) => {
        const result = await axios.post('/register', { username, password });
        return result.data;
    },

    getTasks: async () => {
        const result = await axios.get('/items'); 
        return result.data;
    },

    addTask: async (name) => {
        const result = await axios.post('/items', { name: name, isComplete: false });
        return result.data;
    },

    setCompleted: async (id, isComplete) => {
        const result = await axios.put(`/items/${id}`, { isComplete: isComplete });
        return result.data;
    },

    deleteTask: async (id) => {
        const result = await axios.delete(`/items/${id}`);
        return result.data;
    }
};