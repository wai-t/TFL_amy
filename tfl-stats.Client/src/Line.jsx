import React, { useEffect, useState } from 'react';
import './App.css'; 

const getStatusClass = (description) => {
    const statusMap = {
        good: 'status-good',
        minor: 'status-minordelay',
        severe: 'status-severdelay',
    };

    if (description.includes('Good')) return statusMap.good;
    if (description.includes('Minor')) return statusMap.minor;
    return statusMap.severe;
};

const isDelayed = (statuses) => {
    for (let status of statuses) {
        if (status.statusSeverityDescription.includes('Delay') ||
            status.statusSeverityDescription.includes('Closure') ||
            status.statusSeverityDescription.includes('Suspended')) {
            return true; 
        }
    }
    return false;
};

function LineStatus({ line, index, toggleStatusDetails, expandedStatusRows }) {
    const statuses = line.lineStatuses || [];
    const delayed = isDelayed(statuses);

    return (
        <>
            <tr>
                <td>{line.name}</td>
                <td onClick={() => toggleStatusDetails(index)}>
                    {statuses.map((status, i) => (
                        <div
                            className={getStatusClass(status.statusSeverityDescription)}
                            key={i}
                        >
                            {status.statusSeverityDescription}
                        </div>
                    ))}
                </td>
            </tr>

            {expandedStatusRows[index] && delayed && (
                <tr>
                    <td colSpan="2">
                        {statuses
                            .filter(status => {
                                const keywords = ['Delay', 'Closure', 'Suspended'];
                                return keywords.some(keyword =>
                                    status.statusSeverityDescription.includes(keyword)
                                );
                            })
                            .map((status, i) => (
                                <p key={i}>{status.reason}</p>
                            ))}
                    </td>
                </tr>
            )}
        </>
    );
}

function GetLine() {
    const [lines, setLines] = useState([]);
    const [expandedStatusRows, setExpandedStatusRows] = useState({});
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchLineData = async () => {
            try {
                const response = await fetch('api/LineStatusByMode');
                if (response.ok) {
                    const data = await response.json();
                    setLines(data);
                    setExpandedStatusRows({});
                    setLoading(false);
                } else {
                    setError('Failed to fetch line data.');
                    setLoading(false);
                }
            } catch (error) {
                console.error('Error fetching line data:', error); 
                setError('Error fetching line data:',error);
                setLoading(false);
            }
        };

        fetchLineData();
        const interval = setInterval(fetchLineData, 30000);
        return () => clearInterval(interval); 
    }, []);

    const toggleStatusDetails = (index) => {
        setExpandedStatusRows(prev => ({
            ...prev,
            [index]: !prev[index]
        }));
    };

    const renderTable = () => {
        if (loading) {
            return <p><em>Loading... Please wait.</em></p>;
        }

        if (error) {
            return <p><em>{error}</em></p>;
        }

        return (
            <table className="table" aria-labelledby="tableLabel">
                <tbody>
                    {lines.map((line, index) => (
                        <LineStatus
                            key={index} 
                            line={line}
                            index={index}
                            toggleStatusDetails={toggleStatusDetails}
                            expandedStatusRows={expandedStatusRows}
                        />
                    ))}
                </tbody>
            </table>
        );
    };

    return (
        <div className="container">
            {renderTable()}
        </div>
    );
}

export default GetLine;
