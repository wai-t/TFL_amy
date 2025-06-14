import { useState, useEffect } from 'react';
import './App.css';

function JourneyForm({ formData, fromSuggestions, toSuggestions, onFormSubmit, onInputChange }) {
    return (
        <form className="form journey-form" onSubmit={onFormSubmit}>
            <label htmlFor="from">From:</label>
            <input
                id="from"
                type="text"
                name="from"
                value={formData.from.name}
                onChange={onInputChange}
                required
                list="from-suggestions"
                autoComplete="off"
            />
            <datalist id="from-suggestions">
                {fromSuggestions.map((suggestion) => (
                    <option key={suggestion.naptanId} value={suggestion.commonName} />
                ))}
            </datalist>

            <label htmlFor="to">To:</label>
            <input
                id="to"
                type="text"
                name="to"
                value={formData.to.name}
                onChange={onInputChange}
                required
                list="to-suggestions"
                autoComplete="off"
            />
            <datalist id="to-suggestions">
                {toSuggestions.map((suggestion) => (
                    <option key={suggestion.naptanId} value={suggestion.commonName} />
                ))}
            </datalist>

            <button className="button" type="submit">Plan Journey</button>
        </form>
    );
}

function JourneyDetails({ journeys, noJourneyFound }) {
    if (noJourneyFound) {
        return <p>No journey found. Please try a different route.</p>;
    }

    return (
        <div className="journey-container">
            <h3>Journey Details</h3>
            {journeys.map((journey, index) => (
                <div key={index} className="journey-card">
                    <h4>Journey {index + 1}</h4>
                    {journey.legs.map((leg, legIndex) => (
                        <JourneyLeg key={legIndex} leg={leg} />
                    ))}
                </div>
            ))}
        </div>
    );
}

function JourneyLeg({ leg }) {
    return (
        <div className="journey-leg">
            <p><strong>From:</strong> {leg.departurePoint?.commonName || 'Unknown'}</p>
            <p><strong>To:</strong> {leg.arrivalPoint?.commonName || 'Unknown'}</p>
            <p><strong>Mode:</strong> {leg.mode?.name || 'N/A'}</p>
            <p><strong>Departure:</strong> {leg.departureTime ? new Date(leg.departureTime).toLocaleTimeString() : 'N/A'}</p>
            <p><strong>Arrival:</strong> {leg.arrivalTime ? new Date(leg.arrivalTime).toLocaleTimeString() : 'N/A'}</p>
            <p><strong>Duration:</strong> {leg.duration || 'N/A'} mins</p>
        </div>
    );
}

function GetJourney() {
    const [formData, setFormData] = useState({
        from: { name: '', id: '' },
        to: { name: '', id: '' },
    });
    const [stopPoints, setStopPoints] = useState([])
    const [fromSuggestions, setFromSuggestions] = useState([]);
    const [toSuggestions, setToSuggestions] = useState([]);
    const [journeys, setJourneys] = useState([]);
    const [showJourney, setShowJourney] = useState(false);
    const [noJourneyFound, setNoJourneyFound] = useState(false);
    const [errorMessage, setErrorMessage] = useState('');

    const fetchStopPoints = async () => {
        try {
            const response = await fetch(`api/StopPoint/stopPointList`);
            const data = await response.json();
            setStopPoints(data);
        } catch (error) {
            console.error('Error in fetchStopPoints:', error);
        }
    }

    const handleChange = (e) => {
        const { name, value } = e.target;

        const f = value.toLowerCase();
        let suggestions = [];
        if (value.length > 2) {
            suggestions = stopPoints.filter(s => s.commonName.toLowerCase().includes(f));
        }
        name === "from" ?
            setFromSuggestions(suggestions) : setToSuggestions(suggestions);
        const matched = suggestions.find(s => s.commonName.toLowerCase() === f);
        setFormData((prev) => ({
            ...prev,
            [name]: {
                name: value,
                id: matched ? matched.naptanId : ''
            }
        }));
    };
    const handleSubmit = async (e) => {
        e.preventDefault();
        setShowJourney(false);
        setNoJourneyFound(false);
        setErrorMessage('');

        if (!formData.from.id || !formData.to.id) {
            setErrorMessage('Departure and destination should be selected from suggestions.');
            return;
        }

        if (formData.from.id === formData.to.id) {
            setErrorMessage('Departure and destination cannot be the same.');
            return;
        }

        try {
            const response = await fetch(`api/Journey`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    fromNaptanId: formData.from.id,
                    toNaptanId: formData.to.id,
                }),
            });

            if (!response.ok) {
                const errorMessages = {
                    400: 'Bad request. Please check your input.',
                    404: 'No journey found. Please try again.',
                    500: 'Server error. Please try again later.',
                };
                setErrorMessage(errorMessages[response.status] || `Unexpected error: ${response.status}`);
                if (response.status === 404) setNoJourneyFound(true);
                return;
            }

            const responseData = await response.json();
            setJourneys(responseData);
            setShowJourney(true);
        } catch (error) {
            console.error('Error sending journey data:', error);
            setErrorMessage('Something went wrong. Please check your connection and try again.');
        }
    };

    useEffect(() => {
        fetchStopPoints();
    }, []);

    return (
        <div className="container">
            <h2>Plan Your Journey</h2>

            <JourneyForm
                formData={formData}
                fromSuggestions={fromSuggestions}
                toSuggestions={toSuggestions}
                onFormSubmit={handleSubmit}
                onInputChange={handleChange}
            />

            {errorMessage && (
                <div className="error-message">
                    <p>{errorMessage}</p>
                </div>
            )}

            {showJourney && (
                <JourneyDetails journeys={journeys} noJourneyFound={noJourneyFound} />
            )}
        </div>
    );
}

export default GetJourney;