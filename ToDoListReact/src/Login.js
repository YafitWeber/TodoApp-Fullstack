import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import service from './service';
import './Login.css';

function Login(props) {
    const [isLogin, setIsLogin] = useState(true);
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [msg, setMsg] = useState({ text: "", type: "" });
    
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMsg({ text: "", type: "" }); 
        
        try {
            if (isLogin) {
                // שלב ההתחברות
                const data = await service.login(username, password);
                localStorage.setItem("token", data.token);
                
                if (props.onLogin) {
                    props.onLogin();
                }
                navigate("/");
                
            } else {
                // שלב ההרשמה
                await service.register(username, password);
                setMsg({ text: "נרשמת בהצלחה! כעת ניתן להתחבר", type: "success" });
                setIsLogin(true); 
            }
        } catch (err) {
            console.error("Login/Register Error:", err);

           
            let errorMessage = "פעולה נכשלה. בדקי את החיבור לשרת";
            
            if (err.response && err.response.data) {
                errorMessage = err.response.data.message || 
                              (typeof err.response.data === 'string' ? err.response.data : errorMessage);
            }
            
            setMsg({ text: errorMessage, type: "error" });
        }
    };

    return (
        <div className="login-container">
            <div className="login-card">
                <h2>{isLogin ? "התחברות" : "יצירת חשבון"}</h2>
                <form onSubmit={handleSubmit}>
                    <input 
                        type="text" 
                        placeholder="שם משתמש" 
                        value={username} 
                        onChange={e => setUsername(e.target.value)} 
                        required 
                    />
                    <input 
                        type="password" 
                        placeholder="סיסמה" 
                        value={password} 
                        onChange={e => setPassword(e.target.value)} 
                        required 
                    />
                    
                    {msg.text && (
                        <div className={`message ${msg.type}`} style={{
                            color: msg.type === 'error' ? 'red' : 'green',
                            marginBottom: '10px',
                            fontWeight: 'bold'
                        }}>
                            {msg.text}
                        </div>
                    )}

                    <button type="submit">{isLogin ? "כניסה" : "הרשמה"}</button>
                </form>
                <p className="toggle-auth" style={{ cursor: 'pointer', marginTop: '15px' }} onClick={() => setIsLogin(!isLogin)}>
                    {isLogin ? "עדיין אין לך חשבון? הירשמי" : "כבר יש לך חשבון? התחברי"}
                </p>
            </div>
        </div>
    );
}

export default Login;