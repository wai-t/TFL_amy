import { useState } from 'react';
import './App.css';
import GetLine from './Line';
import GetJourney from './Journey';

function App() {
    const [activeView, setActiveView] = useState('journey');

    return (
        <div>
            <header className="main-header">
                <h1>London Travel Assistant</h1>
                <nav className="navigation">
                    <button
                        className={activeView === 'journey' ? 'nav-button active' : 'nav-button'}
                        onClick={() => setActiveView('journey')}
                    >
                        Journey Planner
                    </button>
                    <button
                        className={activeView === 'lines' ? 'nav-button active' : 'nav-button'}
                        onClick={() => setActiveView('lines')}
                    >
                        Line Status
                    </button>
                </nav>
            </header>

            <div className="content">
                {activeView === 'journey' ? <GetJourney /> : <GetLine />}
            </div>
        </div>
    );
}

export default App;
