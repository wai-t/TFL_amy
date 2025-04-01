import { useState } from 'react';
import './App.css';

function GetJourney() {
    const [formData, setFormData] = useState({ from: '', to: '' });
    const [suggestions, setSuggestions] = useState([]);
    const [showJourney, setShowJourney] = useState(false);
    const [journeys, setJourneys] = useState([]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData({ ...formData, [name]: value });
        if (value.length > 2) {
            fetchSuggestions(value);
        }
    };

    const fetchSuggestions = async (location) => {
        const response = await fetch(`https://localhost:7056/api/StopPoint/autocomplete?query=${location}`);
        const responseData = await response.json();
        setSuggestions(responseData.slice(0, 5));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const response = await fetch('https://localhost:7056/api/Journey', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(formData),
            });
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            const responseData = await response.json();
            setShowJourney(true);
            setJourneys(responseData);
        } catch (error) {
            console.error('Error sending journey data:', error);
        }
    };

    return (
        <div className="container">
            <h2>Plan Your Journey</h2>
            <form className="form" onSubmit={handleSubmit}>
                <label>From:</label>
                <input
                    type="text"
                    name="from"
                    value={formData.from}
                    onChange={handleChange}
                    required
                    list="from-suggestions"
                />
                <datalist id="from-suggestions">
                    {suggestions.map((suggestion, index) => (
                        <option key={index} value={suggestion} />
                    ))}
                </datalist>

                <label>To:</label>
                <input
                    type="text"
                    name="to"
                    value={formData.to}
                    onChange={handleChange}
                    required
                    list="to-suggestions"
                />
                <datalist id="to-suggestions">
                    {suggestions.map((suggestion, index) => (
                        <option key={index} value={suggestion} />
                    ))}
                </datalist>

                <button className="button" type="submit">Plan Journey</button>
            </form>

            {showJourney && journeys.length > 0 && (
                <div className="journey-container">
                    <h3>Journey Details</h3>
                    {journeys.map((journey, index) => (
                        <div key={index} className="journey-card">
                            <h4>Journey {index + 1}</h4>
                            {journey.legs.map((leg, legIndex) => (
                                <div key={legIndex} className="journey-leg">
                                    <p><strong>From:</strong> {leg.departurePoint?.commonName || 'Unknown'}</p>
                                    <p><strong>To:</strong> {leg.arrivalPoint?.commonName || 'Unknown'}</p>
                                    <p><strong>Mode:</strong> {leg.mode?.name || 'N/A'}</p>
                                    <p><strong>Departure:</strong> {leg.departureTime ? new Date(leg.departureTime).toLocaleTimeString() : 'N/A'}</p>
                                    <p><strong>Arrival:</strong> {leg.arrivalTime ? new Date(leg.arrivalTime).toLocaleTimeString() : 'N/A'}</p>
                                    <p><strong>Duration:</strong> {leg.duration || 'N/A'} mins</p>
                                </div>
                            ))}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

export default GetJourney;
